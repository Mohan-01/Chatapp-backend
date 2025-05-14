using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using MongoDB.Driver;
using Shared.Configurations;
using RabbitMQ.Client;
using Serilog.Events;
using Microsoft.OpenApi.Models;
using Shared.Enums.User;
using AuthService.Core.Interfaces;
using AuthService.Infrastructure.Repositories;
using AuthService.Core.Services;
using AuthService.Services;
using AuthService.Infrastructure.Configurations;
using AuthService.Infrastructure.Producers;
using AuthService.API.Middlewares;
using AuthService.API.Logging;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureSerilog(builder);
            ConfigureServices(builder);
            ConfigureAuthentication(builder);
            ConfigureAuthorization(builder);
            ConfigureDatabase(builder);
            ConfigureRabbitMq(builder);
            //ConfigureKestrel(builder);

            var app = builder.Build();
            ConfigureMiddleware(app);
            app.Run();
        }
        private static void ConfigureSerilog(WebApplicationBuilder builder)
        {
            // Setup Serilog from scratch manually
             Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.With<CallerInfoEnricher>()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                //.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message} {MethodName} {ClassName} {FileName} {LineNumber}{NewLine}{Exception}")
                .WriteTo.File("Logs/auth-log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.MongoDB(
                    builder.Configuration["MongoDbConfig:ConnectionString"] + builder.Configuration["MongoDbConfig:DatabaseName"],
                    collectionName: "AuthServiceLogs")
                .CreateLogger();

            builder.Host.UseSerilog();
            Log.Information("Serilog successfully configured.");
        }
        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {your_token}'"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            ConfigureCors(builder);
            ConfigureTokenHandler(builder);
            ConfigureMongoDb(builder);
            ConfigureRepositories(builder);
        }
        private static void ConfigureCors(WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });
        }
        private static void ConfigureTokenHandler(WebApplicationBuilder builder)
        {
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];
            int jwtExpireTime = int.TryParse(builder.Configuration["Jwt:AccessTokenExpiryInDays"], out var expireTime) ? expireTime : 60;
            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new ArgumentException("JWT configuration is missing in appsettings.json");
            }

            builder.Services.AddSingleton<ITokenHandler>(provider => 
                new AuthService.Core.Utils.TokenHandler(jwtKey, jwtIssuer, jwtAudience, jwtExpireTime));
        }
        private static void ConfigureMongoDb(WebApplicationBuilder builder)
        {
            try
            {
                builder.Services.AddSingleton<IMongoClient>(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var connectionString = config["MongoDbConfig:ConnectionString"];
                    Log.Information($"\n\nConnection String: {connectionString}\n\n");
                    var client = new MongoClient(connectionString); // Establish the MongoDB connection
                    return client;
                });

                Log.Information("MongoDB connection successfully established.");
            }
            catch (Exception ex)
            {
                // Log the exception if there is an error establishing the MongoDB connection
                Log.Fatal(ex, "MongoDB connection failed. Error: {Message}", ex.Message);
                throw; // Rethrow the exception to ensure the application stops if MongoDB connection fails
            }
        }
        private static void ConfigureRepositories(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IAuthRepository, AuthRepository>();
            builder.Services.AddSingleton<IAuthService, AuthService.Core.Services.AuthService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();
            builder.Services.AddSingleton<IEmailNotificationService, EmailNotificationService>();
        }
        private static void ConfigureAuthentication(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = async context =>
                    {
                        // Skip validation if endpoint allows anonymous
                        var endpoint = context.HttpContext.GetEndpoint();
                        var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

                        Log.Information($"Allow anonymous: {allowAnonymous}");

                        if (allowAnonymous)
                        {
                            return;
                        }

                        var accessToken = context.Request.Cookies["access_token"];

                        if (!string.IsNullOrEmpty(accessToken) && IsApiRequest(context.Request.Path) && !IsSwaggerRequest(context.Request.Path))
                        {
                            // Validate the token
                            var tokenHandler = context.HttpContext.RequestServices.GetRequiredService<ITokenHandler>();
                            var claimsPrincipal = tokenHandler.ValidateToken(accessToken);

                            if (claimsPrincipal != null)
                            {
                                // Extract username and token version
                                var username = claimsPrincipal.Identity?.Name; // or you can get it from a specific claim if it's named differently
                                var tokenVersionClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "tokenVersion")?.Value;

                                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(tokenVersionClaim))
                                {
                                    context.Response.StatusCode = 401; // Unauthorized
                                    context.Fail("Token is missing username or token version");
                                    return;
                                }

                                // Retrieve the user from the database to compare token version
                                var authRepository = context.HttpContext.RequestServices.GetRequiredService<IAuthRepository>();
                                try
                                {

                                    int tokenVersion = await authRepository.GetTokenVersionWithUsername(username);

                                    // Compare the token version from the token with the stored version in the DB
                                    if (tokenVersion != int.Parse(tokenVersionClaim))
                                    {
                                        context.Response.StatusCode = 401; // Unauthorized
                                        context.Fail("Token version mismatch");
                                        return;
                                    }

                                    // Optional: Log the username and token version for debugging purposes
                                    Log.Information("User {Username} has token version {TokenVersion}", username, tokenVersionClaim);

                                    // Set context properties
                                    context.Principal = claimsPrincipal;
                                    context.Token = accessToken;
                                    context.Success();
                                } catch (Exception)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                context.Response.StatusCode = 401; // Unauthorized
                                context.Fail("Invalid token");
                            }
                        }
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });
        }
        private static void ConfigureAuthorization(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin.ToString()));
                options.AddPolicy("MemberOnly", policy => policy.RequireRole(UserRole.Member.ToString()));
            });
        }
        private static void ConfigureDatabase(WebApplicationBuilder builder)
        {
            builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection("MongoDbConfig"));
            Log.Information("Configured database: {DatabaseName}", builder.Configuration["MongoDbConfig:DatabaseName"]);
        }
        private static void ConfigureRabbitMq(WebApplicationBuilder builder)
        {
            var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQConfig");

            var connectionFactory = new ConnectionFactory
            {
                HostName = rabbitMQConfig["Host"] ?? "localhost",
                Port = int.TryParse(rabbitMQConfig["Port"], out var port) ? port : 5672,
                UserName = rabbitMQConfig["User"] ?? "guest",
                Password = rabbitMQConfig["Password"] ?? "guest",
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = bool.TryParse(rabbitMQConfig["EnableAutoRecovery"], out var recovery) && recovery,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(int.TryParse(rabbitMQConfig["NetworkRecoveryInterval"], out var interval) ? interval : 5)
            };

            builder.Services.AddSingleton<IConnectionFactory>(connectionFactory);
            builder.Services.AddSingleton<IRabbitMQConnection>(sp =>
            {
                var factory = sp.GetRequiredService<IConnectionFactory>();
                var logger = sp.GetRequiredService<ILogger<IRabbitMQConnection>>();
                return new RabbitMQConnection(factory, logger);
            });

            builder.Services.AddSingleton<IEventPublisher, EventPublisher>();
        }
        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseRouting();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseCors("AllowAllOrigins"); // Enable CORS globally
            app.UseSwagger();
            app.UseSwaggerUI();

            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
            {
                app.UseHttpsRedirection();
            }

            app.UseSerilogRequestLogging(); // Logs all HTTP requests automatically
            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseMiddleware<AuthenticationMiddlewear>();

            app.MapControllers();
        }
        private static bool IsApiRequest(PathString path)
        {
            // ✅ Only authenticate API requests, not Swagger, static files, etc.
            return path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
        }
        private static bool IsSwaggerRequest(PathString path)
        {
            return path.StartsWithSegments("/swagger") || path.StartsWithSegments("/swagger/index.html");
        }
    }
}
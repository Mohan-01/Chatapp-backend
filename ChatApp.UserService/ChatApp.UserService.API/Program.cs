using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using RabbitMQ.Client;
using Serilog.Events;
using Serilog;
using Shared.Configurations;
using ChatApp.UserService.Core.Interfaces;
using ChatApp.UserService.Infrastructure.Repositories;
using ChatApp.UserService.Infrastructure.Settings;
using ChatApp.UserService.Infrastructure.Consumers;
using ChatApp.UserService.Infrastructure.BackgroundServices;
using Shared.Middlewares;

namespace ChatApp.UserService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging, database, and services
            ConfigureSerilog(builder);
            ConfigureMongoDb(builder.Services, builder.Configuration);
            ConfigureRabbitMq(builder.Services, builder.Configuration);
            //ConfigureAuthentication(builder);
            ConfigureAuthorization(builder);
            ConfigureCors(builder.Services);
            ConfigureServices(builder.Services);
            ConfigureSwagger(builder.Services);

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure middleware
            ConfigureMiddleware(app);

            app.Run();
        }

        private static void ConfigureSerilog(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/user-log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.MongoDB(builder.Configuration["MongoDbSettings:ConnectionString"] + builder.Configuration["MongoDbSettings:DatabaseName"],
                                 collectionName: "UserServiceLogs")
                .CreateLogger();

            builder.Host.UseSerilog();
        }

        /*
        private static void ConfigureAuthentication(WebApplicationBuilder builder)
        {
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        ValidateLifetime = true
                    };
                });

            Log.Information("JWT Authentication configured for Issuer: {Issuer} and Audience: {Audience}", jwtIssuer, jwtAudience);
        }
        */

        private static void ConfigureAuthorization(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("MemberOnly", policy => policy.RequireRole("Member"));
                options.AddPolicy("InternalOnly", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        var httpContext = context.Resource as HttpContext;
                        if (httpContext == null)
                            return false;

                        if (httpContext.Request.Headers.TryGetValue("X-Internal-Secret", out var secret))
                        {
                            return secret == builder.Configuration["InternalApi:Secret"];
                        }

                        return false;
                    });
                });
            });
        }

        private static void ConfigureCors(IServiceCollection services)
        {
            services.AddCors(options =>
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

        private static void ConfigureMongoDb(IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));
            services.AddSingleton<IMongoDatabase>(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName));
            Log.Information("MongoDB configured with database: {DatabaseName}", settings.DatabaseName);
        }

        private static void ConfigureRabbitMq(IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMQConfig = configuration.GetSection("RabbitMQ");

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

            services.AddSingleton<IConnectionFactory>(_ => connectionFactory);
            services.AddSingleton<IRabbitMQConnection>(sp =>
            {
                var factory = sp.GetRequiredService<IConnectionFactory>();
                var logger = sp.GetRequiredService<ILogger<IRabbitMQConnection>>();
                return new RabbitMQConnection(factory, logger);
            });

            Log.Information("RabbitMQ configured with Host: {Host}", connectionFactory.HostName);

            ConfigureEventConsumers(services);
        }

        private static void ConfigureEventConsumers(IServiceCollection services)
        {
            services.AddSingleton<IUserRegisteredConsumer, UserRegisteredConsumer>();
            services.AddHostedService<UserRegisteredConsumerService>();

            services.AddSingleton<IUsernameChangedConsumer, UsernameChangedConsumer>();
            services.AddHostedService<UsernameChangedConsumerService>();

            services.AddSingleton<IUserDeletedConsumer, UserDeletedConsumer>();
            services.AddHostedService<UserDeletedConsumerService>();

            services.AddSingleton<IEmailChangedConsumer, EmailChangedConsumer>();
            services.AddHostedService<EmailChangedConsumerService>();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<IUserProfileRepository, UserProfileRepository>();
            //services.AddScoped<IUserProfileService, UserProfileService>();

            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, ChatApp.UserService.Core.Services.UserService>();

            services.AddScoped<IUserEventsService, ChatApp.UserService.Core.Services.UserEventsService>();
        }

        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
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
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseCors("AllowAllOrigins"); // Enable CORS globally
            app.UseSwagger();
            app.UseSwaggerUI();
            //app.UseAuthentication();
            app.UseMiddleware<AuthenticationMiddleware>();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
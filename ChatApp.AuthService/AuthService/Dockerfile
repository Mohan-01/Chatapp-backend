# -------- Base runtime image --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

# -------- Build image --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the csproj and restore early (layer caching)
COPY ["AuthService.csproj", "./"]
RUN dotnet restore "AuthService.csproj"

# Copy the rest of the source
COPY . .

# Build the app
RUN dotnet build "AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# -------- Publish image --------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -------- Final runtime image --------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables (RabbitMQ host etc. are better set in docker-compose.yml not here)
ENTRYPOINT ["dotnet", "AuthService.dll"]

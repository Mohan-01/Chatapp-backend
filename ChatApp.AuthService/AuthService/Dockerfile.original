# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore
COPY ["*.sln", "./"]
COPY ["ChatApp.Shared/ChatApp.Shared.csproj", "ChatApp.Shared/"]
COPY ["ChatApp.AuthService/AuthService.Core/AuthService.Core.csproj", "ChatApp.AuthService/AuthService.Core/"]
COPY ["ChatApp.AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "ChatApp.AuthService/AuthService.Infrastructure/"]
COPY ["ChatApp.AuthService/AuthService/AuthService.API.csproj", "ChatApp.AuthService/AuthService/"]

RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR "/src/ChatApp.AuthService/AuthService"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthService.API.dll"]

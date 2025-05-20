# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore
COPY ["*.sln", "./"]
COPY ["ChatApp.Shared/ChatApp.Shared.csproj", "ChatApp.Shared/"]
COPY ["ChatApp.ChatService/ChatService.Core/ChatService.Core.csproj", "ChatApp.ChatService/ChatService.Core/"]
COPY ["ChatApp.ChatService/ChatService.Infrastructure/ChatService.Infrastructure.csproj", "ChatApp.ChatService/ChatService.Infrastructure/"]
COPY ["ChatApp.ChatService/ChatService/ChatService.API.csproj", "ChatApp.ChatService/ChatService/"]

RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR "/src/ChatApp.ChatService/ChatService"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChatService.API.dll"]

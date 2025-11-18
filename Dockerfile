# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get update && apt-get install --no-install-recommends -y curl && rm -rf /var/lib/apt/lists/*

# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and Directory.Build.props first for better caching
COPY ["Directory.Build.props", "."]
COPY ["global.json", "."]
COPY ["UserApi.csproj", "."]
COPY ["UserApi.Tests/UserApi.Tests.csproj", "UserApi.Tests/"]

# Restore dependencies
RUN dotnet restore "./UserApi.csproj"

# Copy source code
COPY . .

# Build the project
RUN dotnet build "UserApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserApi.dll"]

# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["UserApi.csproj", "."]
RUN dotnet restore "./UserApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "UserApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserApi.dll"]

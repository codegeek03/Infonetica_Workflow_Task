# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5191

# Use the SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN addgroup --system --gid 1001 dotnet && \
    adduser --system --uid 1001 --gid 1001 dotnet

USER dotnet

# Configure the app to listen on port 5191
ENV ASPNETCORE_URLS=http://+:5191
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WorkflowEngine.dll"]
# Multi-stage Dockerfile for QuokkaPack.API with optimized layer caching and health checks
# Base runtime image for production
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Build stage with SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for dependency resolution (layer caching optimization)
COPY ["src/QuokkaPack.API/QuokkaPack.API.csproj", "src/QuokkaPack.API/"]
COPY ["src/QuokkaPack.Data/QuokkaPack.Data.csproj", "src/QuokkaPack.Data/"]
COPY ["src/QuokkaPack.Shared/QuokkaPack.Shared.csproj", "src/QuokkaPack.Shared/"]
COPY ["src/QuokkaPack.ServerCommon/QuokkaPack.ServerCommon.csproj", "src/QuokkaPack.ServerCommon/"]

# Restore dependencies (cached layer if project files haven't changed)
RUN dotnet restore "src/QuokkaPack.API/QuokkaPack.API.csproj"

# Copy source code
COPY src/QuokkaPack.API/ src/QuokkaPack.API/
COPY src/QuokkaPack.Data/ src/QuokkaPack.Data/
COPY src/QuokkaPack.Shared/ src/QuokkaPack.Shared/
COPY src/QuokkaPack.ServerCommon/ src/QuokkaPack.ServerCommon/

# Build the application
WORKDIR "/src/src/QuokkaPack.API"
RUN dotnet build "QuokkaPack.API.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "QuokkaPack.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore --no-build /p:UseAppHost=false

# Final production image
FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create logs directory with proper permissions
USER root
RUN mkdir -p /app/Logs && chown -R appuser:appuser /app/Logs
USER appuser

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "QuokkaPack.API.dll"]
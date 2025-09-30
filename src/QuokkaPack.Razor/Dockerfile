# Multi-stage Dockerfile for QuokkaPack.Razor with optimized static asset handling and health checks
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

# Development stage (for development support)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS development
WORKDIR /src
EXPOSE 8080
EXPOSE 8081

# Install Node.js for potential frontend tooling
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs

# Copy project files for development
COPY . .
WORKDIR "/src/src/QuokkaPack.Razor"
RUN dotnet restore
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]

# Build stage with SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for dependency resolution (layer caching optimization)
COPY ["src/QuokkaPack.Razor/QuokkaPack.Razor.csproj", "src/QuokkaPack.Razor/"]
COPY ["src/QuokkaPack.Shared/QuokkaPack.Shared.csproj", "src/QuokkaPack.Shared/"]
COPY ["src/QuokkaPack.ServerCommon/QuokkaPack.ServerCommon.csproj", "src/QuokkaPack.ServerCommon/"]

# Restore dependencies (cached layer if project files haven't changed)
RUN dotnet restore "src/QuokkaPack.Razor/QuokkaPack.Razor.csproj"

# Copy source code (only necessary projects)
COPY src/QuokkaPack.Razor/ src/QuokkaPack.Razor/
COPY src/QuokkaPack.Shared/ src/QuokkaPack.Shared/
COPY src/QuokkaPack.ServerCommon/ src/QuokkaPack.ServerCommon/

# Build the application
WORKDIR "/src/src/QuokkaPack.Razor"
RUN dotnet build "QuokkaPack.Razor.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish stage with static asset optimization
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/src/QuokkaPack.Razor"

# Publish with static web asset optimization
RUN dotnet publish "QuokkaPack.Razor.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore --no-build /p:UseAppHost=false

# Ensure static assets are properly included
RUN if [ -d "./bin/$BUILD_CONFIGURATION/net9.0/staticwebassets.packaged" ]; then \
        mkdir -p /app/publish/wwwroot && \
        cp -r ./bin/$BUILD_CONFIGURATION/net9.0/staticwebassets.packaged/* /app/publish/wwwroot/ || true; \
    fi

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

ENTRYPOINT ["dotnet", "QuokkaPack.Razor.dll"]

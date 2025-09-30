# Multi-stage Dockerfile for QuokkaPack.Blazor WebAssembly
# Base image for serving static files
FROM nginx:alpine AS base
WORKDIR /usr/share/nginx/html
EXPOSE 80

# Build stage - .NET SDK for building Blazor WebAssembly
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/QuokkaPack.Blazor/QuokkaPack.Blazor.csproj", "src/QuokkaPack.Blazor/"]
COPY ["src/QuokkaPack.Shared/QuokkaPack.Shared.csproj", "src/QuokkaPack.Shared/"]
RUN dotnet restore "./src/QuokkaPack.Blazor/QuokkaPack.Blazor.csproj"

# Copy source code
COPY src/QuokkaPack.Blazor/ src/QuokkaPack.Blazor/
COPY src/QuokkaPack.Shared/ src/QuokkaPack.Shared/

# Build the Blazor WebAssembly project
WORKDIR "/src/src/QuokkaPack.Blazor"
RUN dotnet build "./QuokkaPack.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage - Create optimized WebAssembly output
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/src/QuokkaPack.Blazor"
RUN dotnet publish "./QuokkaPack.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage - nginx serving static Blazor WebAssembly files
FROM base AS final
WORKDIR /usr/share/nginx/html

# Copy published Blazor WebAssembly files
COPY --from=publish /app/publish/wwwroot .

# Copy custom nginx configuration for Blazor WebAssembly
COPY src/QuokkaPack.Blazor/nginx.conf /etc/nginx/nginx.conf

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup && \
    chown -R appuser:appgroup /usr/share/nginx/html && \
    chown -R appuser:appgroup /var/cache/nginx && \
    chown -R appuser:appgroup /var/log/nginx && \
    chown -R appuser:appgroup /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown -R appuser:appgroup /var/run/nginx.pid

USER appuser

# Health check for nginx
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost/ || exit 1

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
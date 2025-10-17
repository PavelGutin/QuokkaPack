# Stage 1: Build Angular application
FROM node:20-alpine AS angular-build

WORKDIR /app

# Copy Angular project files
COPY src/QuokkaPack.Angular/package*.json ./
RUN npm ci

COPY src/QuokkaPack.Angular/ ./
RUN npm run build

# Stage 2: Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-build

WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/QuokkaPack.API/QuokkaPack.API.csproj", "src/QuokkaPack.API/"]
COPY ["src/QuokkaPack.Data/QuokkaPack.Data.csproj", "src/QuokkaPack.Data/"]
COPY ["src/QuokkaPack.Shared/QuokkaPack.Shared.csproj", "src/QuokkaPack.Shared/"]
COPY ["src/QuokkaPack.ServerCommon/QuokkaPack.ServerCommon.csproj", "src/QuokkaPack.ServerCommon/"]

RUN dotnet restore "src/QuokkaPack.API/QuokkaPack.API.csproj"

# Install dotnet-ef tool for migrations
RUN dotnet tool install --global dotnet-ef

# Copy source code and build
COPY src/QuokkaPack.API/ src/QuokkaPack.API/
COPY src/QuokkaPack.Data/ src/QuokkaPack.Data/
COPY src/QuokkaPack.Shared/ src/QuokkaPack.Shared/
COPY src/QuokkaPack.ServerCommon/ src/QuokkaPack.ServerCommon/

WORKDIR "/src/src/QuokkaPack.API"
RUN dotnet publish "QuokkaPack.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generate migration bundle for runtime execution
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --configuration Release -o /app/efbundle --self-contained

# Stage 3: Final runtime image with nginx and dotnet
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Install nginx and supervisor
RUN apt-get update && \
    apt-get install -y nginx supervisor && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Copy supervisord configuration
COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

# Copy entrypoint script and ensure Unix line endings
COPY entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

# Copy Angular build output to nginx html directory
COPY --from=angular-build /app/dist/QuokkaPack.Angular/browser /usr/share/nginx/html

# Copy .NET API build output
COPY --from=api-build /app/publish /app

# Copy the EF migration bundle
COPY --from=api-build /app/efbundle /app/efbundle

# Create directory for SQLite database
RUN mkdir -p /app/data && chmod 777 /app/data

# Expose port 80
EXPOSE 80

# Set environment variables for the API
ENV ASPNETCORE_URLS=http://localhost:5000
ENV ASPNETCORE_ENVIRONMENT=Docker

# Use entrypoint script that runs migrations before starting supervisor
ENTRYPOINT ["/entrypoint.sh"]

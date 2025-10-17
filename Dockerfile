# Stage 1: Build .NET API and generate OpenAPI spec
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

# Copy source code
COPY src/QuokkaPack.API/ src/QuokkaPack.API/
COPY src/QuokkaPack.Data/ src/QuokkaPack.Data/
COPY src/QuokkaPack.Shared/ src/QuokkaPack.Shared/
COPY src/QuokkaPack.ServerCommon/ src/QuokkaPack.ServerCommon/

# Build API to generate OpenAPI spec
WORKDIR "/src/src/QuokkaPack.API"
RUN dotnet build "QuokkaPack.API.csproj" -c Release

# Generate OpenAPI spec
RUN dotnet run --no-build --configuration Release --urls "http://localhost:5000" & \
    sleep 5 && \
    curl -o /src/openapi.json http://localhost:5000/openapi/v1.json && \
    pkill -f QuokkaPack.API || true

# Stage 2: Generate TypeScript API client
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS codegen

WORKDIR /codegen

# Install NSwag.ConsoleCore
RUN dotnet tool install --global NSwag.ConsoleCore
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy OpenAPI spec and nswag config
COPY --from=api-build /src/openapi.json ./openapi.json
COPY src/QuokkaPack.Angular/codegen/nswag.json ./nswag.json

# Update nswag config to use local openapi.json and correct runtime
RUN sed -i 's|../../../artifacts/openapi.json|./openapi.json|g' nswag.json && \
    sed -i 's|"../src/app/api/api-client.ts"|"./api-client.ts"|g' nswag.json

# Generate TypeScript client
RUN nswag run nswag.json

# Stage 3: Build Angular application
FROM node:20-alpine AS angular-build

WORKDIR /app

# Copy Angular project files
COPY src/QuokkaPack.Angular/package*.json ./
RUN npm ci

COPY src/QuokkaPack.Angular/ ./

# Copy generated API client
COPY --from=codegen /codegen/api-client.ts ./src/app/api/api-client.ts

# Build Angular app
RUN npm run build

# Stage 4: Publish .NET API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-publish

WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/QuokkaPack.API/QuokkaPack.API.csproj", "src/QuokkaPack.API/"]
COPY ["src/QuokkaPack.Data/QuokkaPack.Data.csproj", "src/QuokkaPack.Data/"]
COPY ["src/QuokkaPack.Shared/QuokkaPack.Shared.csproj", "src/QuokkaPack.Shared/"]
COPY ["src/QuokkaPack.ServerCommon/QuokkaPack.ServerCommon.csproj", "src/QuokkaPack.ServerCommon/"]

RUN dotnet restore "src/QuokkaPack.API/QuokkaPack.API.csproj"

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef

# Copy source code
COPY src/QuokkaPack.API/ src/QuokkaPack.API/
COPY src/QuokkaPack.Data/ src/QuokkaPack.Data/
COPY src/QuokkaPack.Shared/ src/QuokkaPack.Shared/
COPY src/QuokkaPack.ServerCommon/ src/QuokkaPack.ServerCommon/

# Publish API
WORKDIR "/src/src/QuokkaPack.API"
RUN dotnet publish "QuokkaPack.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generate migration bundle
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --configuration Release -o /app/efbundle --self-contained

# Stage 5: Final runtime image with nginx and dotnet
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
COPY --from=api-publish /app/publish /app

# Copy the EF migration bundle
COPY --from=api-publish /app/efbundle /app/efbundle

# Create directory for SQLite database
RUN mkdir -p /app/data && chmod 777 /app/data

# Expose port 80
EXPOSE 80

# Set environment variables for the API
ENV ASPNETCORE_URLS=http://localhost:5000
ENV ASPNETCORE_ENVIRONMENT=Docker

# Use entrypoint script that runs migrations before starting supervisor
ENTRYPOINT ["/entrypoint.sh"]

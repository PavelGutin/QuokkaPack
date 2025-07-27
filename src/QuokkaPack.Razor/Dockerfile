### C:\repos\QuokkaPack\src\QuokkaPack.Razor\Dockerfile

# Base image used at runtime (production)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore
COPY ["src/QuokkaPack.Razor/QuokkaPack.Razor.csproj", "src/QuokkaPack.Razor/"]
COPY ["src/QuokkaPack.Shared/QuokkaPack.Shared.csproj", "src/QuokkaPack.Shared/"]
RUN dotnet restore "./src/QuokkaPack.Razor/QuokkaPack.Razor.csproj"

# Copy entire solution
COPY . .

# Build the project
WORKDIR "/src/src/QuokkaPack.Razor"
RUN dotnet build "./QuokkaPack.Razor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the project and copy staticwebassets.packaged to wwwroot
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/src/QuokkaPack.Razor"
RUN dotnet publish "./QuokkaPack.Razor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false && \
    mkdir -p /app/publish/wwwroot && \
    cp -r ./bin/$BUILD_CONFIGURATION/net9.0/staticwebassets.packaged/* /app/publish/wwwroot/

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chown -R $APP_UID:$APP_UID /app
ENTRYPOINT ["dotnet", "QuokkaPack.Razor.dll"]

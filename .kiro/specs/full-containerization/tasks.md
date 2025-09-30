# Implementation Plan

- [x] 1. Integrate Angular project into Visual Studio solution





  - Create QuokkaPack.Angular.esproj file with MSBuild integration for Node.js
  - Add Angular project reference to QuokkaPack.sln solution file
  - Configure project references to QuokkaPack.Shared for model sharing
  - Update Directory.Build.props to include JavaScript SDK support
  - Implement TypeScript type generation from Data project models to types.gen.ts
  - Configure build process to automatically regenerate types when Data models change
  - Update api-types.ts to import generated types from types.gen.ts
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7, 7.8_

- [x] 2. Create Blazor project Dockerfile





  - Write multi-stage Dockerfile for QuokkaPack.Blazor with .NET 9.0 base images
  - Implement build stage with project dependencies and NuGet restore
  - Add publish stage with Blazor Server optimizations and static asset handling
  - Configure final runtime stage with proper user permissions and health checks
  - _Requirements: 1.3, 2.1, 2.2, 2.3, 2.4_

- [x] 3. Create Angular project Dockerfile





  - Write multi-stage Dockerfile with Node.js build stage and nginx runtime stage
  - Implement Node.js build stage with npm install and Angular production build
  - Configure nginx stage with Angular asset serving and API proxy configuration
  - Add health checks and proper file permissions for container security
  - _Requirements: 1.4, 2.1, 2.2, 2.3, 2.4_

- [x] 4. Enhance existing API and Razor Dockerfiles





  - Update API Dockerfile with improved multi-stage build and layer caching optimization
  - Enhance Razor Dockerfile with better static asset handling and development support
  - Add comprehensive health checks to both Dockerfiles
  - Optimize .dockerignore patterns for better build performance
  - _Requirements: 1.1, 1.2, 1.5, 2.1, 2.2, 2.3, 2.4_

- [x] 5. Create development Docker Compose configuration





  - Write docker-compose.dev.yml with all four frontend services and SQL Server
  - Configure volume mounts for source code hot reload in development
  - Set up development-specific environment variables and debug port exposure
  - Add relaxed health checks and development database seeding
  - _Requirements: 3.1, 3.3, 4.1, 4.2, 4.3, 5.1, 5.2_

- [x] 6. Create production Docker Compose configuration





  - Write docker-compose.prod.yml with optimized production settings
  - Configure production environment variables and resource constraints
  - Implement strict health checks with proper restart policies
  - Set up production database persistence and backup volume configuration
  - _Requirements: 3.2, 3.4, 4.1, 4.4, 4.5, 5.1, 5.3, 5.4_

- [x] 7. Implement database containerization enhancements





  - Update SQL Server container configuration with proper health checks
  - Create database initialization scripts for automatic migration execution
  - Configure Docker volumes for persistent database storage
  - Add retry logic for database connection handling in application startup
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

- [x] 8. Create Razor self-host all-in-one Dockerfile





  - Write Dockerfile.selfhost.razor combining API, Razor frontend, and SQLite database
  - Implement automatic database migration and seeding on container startup
  - Configure nginx reverse proxy for API routing and static file serving
  - Add volume mount configuration for data persistence and environment variable support
  - _Requirements: 8.1, 8.4, 8.5, 8.6, 8.7, 8.8_

- [x] 9. Create Blazor self-host all-in-one Dockerfile





  - Write Dockerfile.selfhost.blazor combining API, Blazor Server, and SQLite database
  - Implement SignalR hub configuration for Blazor Server functionality
  - Configure automatic database initialization with migration and seed data
  - Add data persistence volumes and simplified environment variable configuration
  - _Requirements: 8.2, 8.4, 8.5, 8.6, 8.7, 8.8_

- [x] 10. Create Angular self-host all-in-one Dockerfile





  - Write Dockerfile.selfhost.angular combining API, Angular SPA, and SQLite database
  - Configure nginx for Angular asset serving and API proxy routing
  - Implement automatic database setup with SQLite initialization
  - Add volume configuration for persistent data and single-port application serving
  - _Requirements: 8.3, 8.4, 8.5, 8.6, 8.7, 8.8_

- [x] 11. Implement SQLite database configuration for self-hosting





  - Create SQLite-specific DbContext configuration and connection string handling
  - Write database initialization service for automatic migration and seeding
  - Implement data persistence logic with proper file path management
  - Add configuration switching between SQL Server and SQLite based on environment
  - _Requirements: 8.5, 8.6_

- [x] 12. Create container management scripts and documentation








  - Write PowerShell scripts for building all Docker images with single command
  - Create scripts for starting development and production environments
  - Implement container cleanup and maintenance scripts
  - Write comprehensive documentation for container deployment and troubleshooting
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 13. Add comprehensive health checks and monitoring





  - Implement health check endpoints in all web applications
  - Configure container health checks with appropriate timeouts and retry logic
  - Add logging configuration for container-friendly structured logging
  - Create monitoring and metrics collection setup for containerized deployments
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [x] 14. Write automated tests for container functionality





  - Create integration tests for Docker Compose development and production environments
  - Write tests for self-host container deployment and functionality verification
  - Implement automated build verification tests for all Dockerfiles
  - Add performance tests for container startup time and resource usage
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 8.1, 8.2, 8.3_
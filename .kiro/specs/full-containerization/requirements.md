# Requirements Document

## Introduction

This feature will enhance the existing containerization setup by providing comprehensive Docker support for all QuokkaPack projects. Currently, the project has basic Docker Compose configuration, but we need to extend this to include proper Dockerfiles for all deployable projects, optimized multi-stage builds, development and production configurations, and complete container orchestration for the entire application stack. This includes properly integrating the Angular project into the Visual Studio solution structure before containerizing it.

## Requirements

### Requirement 1

**User Story:** As a developer, I want all QuokkaPack projects to have proper Dockerfiles, so that I can build and deploy any component independently in a containerized environment.

#### Acceptance Criteria

1. WHEN building the API project THEN the system SHALL create a Docker image with .NET 9.0 runtime and all necessary dependencies
2. WHEN building the Razor Pages project THEN the system SHALL create a Docker image optimized for web serving with proper static file handling
3. WHEN building the Blazor project THEN the system SHALL create a Docker image with Blazor Server runtime configuration
4. WHEN the Angular project is properly integrated into the Visual Studio solution THEN the system SHALL create a Docker image with Node.js runtime and built Angular assets
5. IF a project has dependencies on shared libraries THEN the Docker build SHALL include all necessary project references

### Requirement 2

**User Story:** As a developer, I want optimized multi-stage Docker builds, so that production images are minimal and secure while development images include debugging tools.

#### Acceptance Criteria

1. WHEN building for production THEN the system SHALL use multi-stage builds to minimize final image size
2. WHEN building for development THEN the system SHALL include debugging symbols and development tools
3. WHEN copying application files THEN the system SHALL exclude unnecessary files using .dockerignore
4. WHEN building images THEN the system SHALL use appropriate base images for each project type
5. IF building multiple projects THEN the system SHALL reuse common layers to optimize build time

### Requirement 3

**User Story:** As a developer, I want separate Docker Compose configurations for development and production, so that I can run appropriate environments for different scenarios.

#### Acceptance Criteria

1. WHEN running in development mode THEN the system SHALL mount source code volumes for hot reload
2. WHEN running in production mode THEN the system SHALL use built images without volume mounts
3. WHEN starting the development environment THEN the system SHALL include debugging ports and development databases
4. WHEN starting the production environment THEN the system SHALL use optimized configurations and health checks
5. IF environment variables are needed THEN the system SHALL support .env files for configuration

### Requirement 4

**User Story:** As a developer, I want complete database containerization, so that I can run the entire application stack without external dependencies.

#### Acceptance Criteria

1. WHEN starting the application stack THEN the system SHALL include SQL Server container for production scenarios
2. WHEN running in development THEN the system SHALL support both SQL Server and SQLite options
3. WHEN initializing the database THEN the system SHALL automatically run Entity Framework migrations
4. WHEN persisting data THEN the system SHALL use Docker volumes for database storage
5. IF the database container fails THEN the system SHALL provide proper health checks and restart policies

### Requirement 5

**User Story:** As a DevOps engineer, I want container health checks and monitoring, so that I can ensure application reliability in containerized deployments.

#### Acceptance Criteria

1. WHEN containers are running THEN the system SHALL provide health check endpoints for all services
2. WHEN a service becomes unhealthy THEN the system SHALL automatically restart the container
3. WHEN monitoring the application THEN the system SHALL expose metrics and logs in container-friendly formats
4. WHEN scaling services THEN the system SHALL support horizontal scaling through Docker Compose
5. IF a dependency service fails THEN the system SHALL implement proper retry logic and graceful degradation

### Requirement 6

**User Story:** As a developer, I want simplified container management commands, so that I can easily build, run, and maintain the containerized application.

#### Acceptance Criteria

1. WHEN building all services THEN the system SHALL provide a single command to build all Docker images
2. WHEN starting the application THEN the system SHALL provide commands for both development and production modes
3. WHEN updating the application THEN the system SHALL support rolling updates without downtime
4. WHEN troubleshooting THEN the system SHALL provide easy access to container logs and debugging
5. IF cleaning up resources THEN the system SHALL provide commands to remove containers, images, and volumes

### Requirement 7

**User Story:** As a developer, I want the Angular project properly integrated into the Visual Studio solution, so that it follows the same build and deployment patterns as other QuokkaPack projects.

#### Acceptance Criteria

1. WHEN building the solution THEN the system SHALL include the Angular project in the build process
2. WHEN the Angular project is built THEN the system SHALL integrate with the existing MSBuild pipeline
3. WHEN referencing shared libraries THEN the Angular project SHALL have access to QuokkaPack.Shared models and DTOs
4. WHEN developing locally THEN the Angular project SHALL follow the same project structure conventions as other QuokkaPack projects
5. IF the Angular project needs Node.js dependencies THEN the system SHALL manage them through the solution build process
6. WHEN building the Angular project THEN the system SHALL automatically generate types.gen.ts from Data project models
7. WHEN types.gen.ts is generated THEN the system SHALL make the types available for import in api-types.ts
8. IF Data project models change THEN the system SHALL regenerate TypeScript types as part of the build process
5. IF the Angular project needs Node.js dependencies THEN the system SHALL manage them through the solution build process### Re
quirement 8

**User Story:** As a self-hosting user, I want all-in-one Docker images for each frontend option, so that I can easily deploy QuokkaPack in my home lab environment with a single container.

#### Acceptance Criteria

1. WHEN building the Razor self-host image THEN the system SHALL create a single container with API, database, and Razor frontend
2. WHEN building the Blazor self-host image THEN the system SHALL create a single container with API, database, and Blazor frontend  
3. WHEN building the Angular self-host image THEN the system SHALL create a single container with API, database, and Angular frontend
4. WHEN starting a self-host container THEN the system SHALL automatically initialize the database with migrations and seed data
5. WHEN running self-host containers THEN the system SHALL use SQLite for simplified database management
6. IF a self-host container is restarted THEN the system SHALL persist user data and application state
7. WHEN deploying self-host images THEN the system SHALL provide simple configuration through environment variables
8. WHEN accessing the self-host application THEN the system SHALL serve the complete application on a single port
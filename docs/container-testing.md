# Container Testing Guide

This document describes the automated testing framework for QuokkaPack's containerization functionality.

## Overview

The container testing suite provides comprehensive verification of Docker containers, Docker Compose configurations, and self-host deployments. Tests are organized into four main categories:

- **Build Verification Tests**: Verify all Dockerfiles build successfully
- **Integration Tests**: Test Docker Compose environments and service communication
- **Self-Host Tests**: Validate all-in-one container deployments
- **Performance Tests**: Measure container startup time and resource usage

## Test Categories

### Build Verification Tests

Located in `BuildVerificationTests.cs`, these tests verify:

- All individual project Dockerfiles build successfully
- Self-host Dockerfiles build without errors
- Image sizes are within reasonable limits
- Images have proper configuration and labels
- Build performance is acceptable

**Key Tests:**
- `API_Dockerfile_Should_Build_Successfully`
- `Razor_Dockerfile_Should_Build_Successfully`
- `Blazor_Dockerfile_Should_Build_Successfully`
- `Angular_Dockerfile_Should_Build_Successfully`
- `SelfHost_*_Dockerfile_Should_Build_Successfully`

### Integration Tests

Located in `DockerComposeIntegrationTests.cs`, these tests verify:

- Development and production Docker Compose environments start successfully
- All services become healthy within expected timeframes
- Services can communicate with each other
- Health check endpoints are accessible
- Web applications serve content correctly
- Database connections work properly

**Key Tests:**
- `Development_Environment_Should_Start_Successfully`
- `Production_Environment_Should_Start_Successfully`
- `API_Service_Should_Respond_To_Health_Checks`
- `Services_Should_Communicate_With_Each_Other`

### Self-Host Tests

Located in `SelfHostContainerTests.cs`, these tests verify:

- All-in-one containers start and serve applications
- Database initialization works automatically
- Data persistence across container restarts
- Environment variable configuration
- Volume mounting and data storage

**Key Tests:**
- `SelfHost_Razor_Container_Should_Start_And_Serve_Application`
- `SelfHost_Containers_Should_Initialize_Database_Automatically`
- `SelfHost_Container_Should_Persist_Data_Across_Restarts`

### Performance Tests

Located in `ContainerPerformanceTests.cs`, these tests verify:

- Container startup times are within acceptable limits
- Resource usage (CPU/Memory) stays within bounds
- Multiple containers can start concurrently
- Performance metrics are collected and validated

**Key Tests:**
- `API_Container_Startup_Performance_Should_Be_Acceptable`
- `Container_Resource_Usage_Should_Be_Within_Limits`
- `Multiple_Containers_Should_Start_Concurrently`

## Running Tests

### Local Development

Use the PowerShell script to run tests locally:

```powershell
# Run all container tests
.\scripts\test-containers.ps1

# Run specific test category
.\scripts\test-containers.ps1 -TestCategory BuildVerification
.\scripts\test-containers.ps1 -TestCategory Integration
.\scripts\test-containers.ps1 -TestCategory SelfHost
.\scripts\test-containers.ps1 -TestCategory Performance

# Run with verbose output
.\scripts\test-containers.ps1 -Verbose
```

### Using .NET CLI

Run tests directly with dotnet:

```bash
# Build the test project
dotnet build tests/QuokkaPack.ContainerTests/QuokkaPack.ContainerTests.csproj

# Run all container tests
dotnet test tests/QuokkaPack.ContainerTests/QuokkaPack.ContainerTests.csproj

# Run specific test class
dotnet test tests/QuokkaPack.ContainerTests/QuokkaPack.ContainerTests.csproj --filter "FullyQualifiedName~BuildVerificationTests"

# Run with detailed output
dotnet test tests/QuokkaPack.ContainerTests/QuokkaPack.ContainerTests.csproj --verbosity detailed
```

### CI/CD Pipeline

Tests run automatically in GitHub Actions:

- **Build Verification**: Runs on all pushes and PRs
- **Integration Tests**: Runs on all pushes and PRs
- **Self-Host Tests**: Runs on all pushes and PRs
- **Performance Tests**: Runs only on main branch pushes

## Prerequisites

### Local Development

- Docker Desktop installed and running
- Docker Compose available
- .NET 9.0 SDK installed
- Node.js 18+ (for Angular builds)
- PowerShell (for test scripts)

### CI Environment

- Ubuntu latest runner
- Docker and Docker Compose pre-installed
- .NET 9.0 SDK
- Node.js 18

## Test Infrastructure

### Base Classes

**DockerTestBase**: Provides common Docker operations
- Building images from Dockerfiles
- Running containers with configuration
- Health check monitoring
- Container cleanup

**DockerComposeTestHelper**: Manages Docker Compose operations
- Starting/stopping services
- Health monitoring
- Log collection
- Port mapping discovery

**PerformanceTestHelper**: Measures container performance
- Startup time measurement
- Resource usage monitoring
- Performance metrics collection

### Test Configuration

Tests are configured to run sequentially (not in parallel) to avoid Docker resource conflicts:

```json
{
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false,
  "maxParallelThreads": 1
}
```

## Troubleshooting

### Common Issues

**Docker not available**: Ensure Docker Desktop is running
```bash
docker --version
docker-compose --version
```

**Port conflicts**: Tests use dynamic port allocation, but conflicts can occur
- Stop other Docker containers
- Check for processes using test ports (8090-8099)

**Build failures**: Ensure all dependencies are available
```bash
dotnet restore
npm install --prefix src/QuokkaPack.Angular
```

**Permission issues**: On Linux, ensure Docker can be run without sudo
```bash
sudo usermod -aG docker $USER
```

### Debugging Tests

Enable verbose logging:
```bash
dotnet test --verbosity detailed --logger console
```

Check Docker logs:
```bash
docker logs <container-id>
docker-compose logs <service-name>
```

Clean up test resources:
```bash
docker system prune -f
docker volume prune -f
```

## Performance Expectations

### Build Times
- Individual Dockerfiles: < 10 minutes
- Self-host Dockerfiles: < 15 minutes

### Startup Times
- API containers: < 2 minutes
- Frontend containers: < 2 minutes
- Self-host containers: < 3 minutes (includes DB initialization)

### Resource Usage
- API containers: < 512MB memory, < 50% CPU average
- Frontend containers: < 256MB memory, < 30% CPU average
- Self-host containers: < 1GB memory, < 60% CPU average

## Extending Tests

### Adding New Test Cases

1. Create test method in appropriate test class
2. Use base class helpers for Docker operations
3. Follow naming convention: `Component_Should_Behavior`
4. Add proper assertions with FluentAssertions
5. Include cleanup in test disposal

### Adding New Test Categories

1. Create new test class inheriting from `DockerTestBase`
2. Add category to PowerShell script
3. Update GitHub Actions workflow
4. Document new tests in this guide

### Custom Assertions

Use FluentAssertions for readable test assertions:

```csharp
response.StatusCode.Should().Be(HttpStatusCode.OK);
metrics.TotalStartupTime.Should().BeLessThan(TimeSpan.FromMinutes(2));
containerIds.Should().AllSatisfy(id => id.Should().NotBeNullOrEmpty());
```

## Best Practices

1. **Cleanup**: Always dispose test resources properly
2. **Isolation**: Each test should be independent
3. **Timeouts**: Use reasonable timeouts for container operations
4. **Logging**: Include informative log messages
5. **Assertions**: Use descriptive assertion messages
6. **Performance**: Monitor test execution time
7. **Reliability**: Handle flaky Docker operations with retries
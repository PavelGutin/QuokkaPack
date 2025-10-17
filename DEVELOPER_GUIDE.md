# 🛠️ QuokkaPack Developer Guide

This guide covers everything you need to set up, run, and develop QuokkaPack locally.

---

## 🐳 Quick Start with Docker (Easiest)

The fastest way to run QuokkaPack is using the pre-built Docker image:

```bash
docker pull ghcr.io/pavelgutin/quokkapack:latest
docker run -d -p 8080:8080 \
  -e JWT_SECRET="your-super-secret-key-min-32-chars" \
  ghcr.io/pavelgutin/quokkapack:latest
```

Then access the application at `http://localhost:8080`

**For full Docker Compose setup with database**, see the [Docker Deployment](#docker-deployment) section below.

---

## Prerequisites

Before you begin, ensure you have the following installed:

- **[.NET 9 SDK](https://dotnet.microsoft.com/download)** - Required for the ASP.NET Core API
- **[Node.js 20+](https://nodejs.org/)** and npm - Required for Angular development
- **[SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)** or SQL Server instance
- **[PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell)** - For codegen and seeding scripts (cross-platform)
- **[Git](https://git-scm.com/)** - For version control

---

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/PavelGutin/QuokkaPack.git
cd QuokkaPack
```

### 2. Set Up Environment Variables

Create a `.env` file in the project root (optional, mainly for Docker):

```bash
# Copy the example .env file if it exists, or create one manually
cp .env.example .env
```

Edit `.env` and set your JWT secret (minimum 32 characters):

```env
JWT_SECRET=your-super-secret-key-here-minimum-32-characters-required
```

**Note:** For local development, it's recommended to use .NET user secrets instead (see step 4).

### 3. Set Up the Database

Navigate to the API project and apply EF Core migrations:

```bash
cd src/QuokkaPack.API
dotnet ef database update
```

This will create the `QuokkaPackDb` database in your LocalDB instance with all required tables.

### 4. Configure JWT Secrets

**For Development (Recommended):**

Use .NET user secrets to store your JWT secret securely:

```bash
cd src/QuokkaPack.API
dotnet user-secrets set "JwtSettings:Secret" "your-super-secret-key-here-min-32-chars"
```

User secrets are stored outside your source code and won't be committed to Git.

**For Production:**

Configure via environment variables or `appsettings.Production.json` (never commit secrets to source control!):

```bash
export JwtSettings__Secret="your-production-secret-min-32-chars"
export ConnectionStrings__DefaultConnection="your-production-connection-string"
```

### 5. Start the API

```bash
cd src/QuokkaPack.API
dotnet run
```

The API will be available at:
- **HTTPS:** `https://localhost:7100`
- **HTTP:** `http://localhost:5000`
- **Swagger UI:** `https://localhost:7100/swagger` (development mode only)

### 6. Start the Angular App

In a new terminal:

```bash
cd src/QuokkaPack.Angular
npm install
npm start
```

The Angular app will be available at:
- **URL:** `http://localhost:4200`
- **API Proxy:** Requests to `/api/*` are automatically proxied to `https://localhost:7100`

### 7. Seed Demo Data (Optional)

After the API is running, you can populate the database with demo data:

**PowerShell (Windows/macOS/Linux):**
```bash
pwsh tools/seed-demo-data.ps1
```

**Bash (macOS/Linux):**
```bash
bash tools/seed-demo-data.sh
```

This creates:
- Demo user: `demo@quokkapack.com` / `Demo123!`
- 6 categories (Toiletries, Clothing, Electronics, Documents, Outdoor Gear, Health & Safety)
- 43 items across categories
- 5 sample trips

**Login to the app** with the demo credentials to see the sample data.

---

## Development Workflow

### Database Migrations

When you modify entity models in `QuokkaPack.Shared/Models/`, create and apply migrations:

#### Create a New Migration

```bash
cd src/QuokkaPack.API
dotnet ef migrations add MigrationName
```

This generates migration files in `QuokkaPack.Data/Migrations/`.

#### Apply Migrations

```bash
dotnet ef database update
```

#### Rollback to a Previous Migration

```bash
dotnet ef database update PreviousMigrationName
```

#### Remove the Last Migration (if not applied)

```bash
dotnet ef migrations remove
```

**Important:** Always review migration files before applying them to ensure they match your intended schema changes.

---

### API Client Code Generation

When you modify API contracts (DTOs, models, controllers), regenerate the TypeScript API client for Angular:

#### Automatic Regeneration

The OpenAPI spec is automatically regenerated when the API builds (incremental - only when DTOs/Controllers change).

#### Manual Regeneration

**From Angular project:**
```bash
cd src/QuokkaPack.Angular
npm run codegen
```

**Using scripts directly:**

**PowerShell (cross-platform):**
```bash
pwsh tools/generate-openapi.ps1
```

**Bash (macOS/Linux):**
```bash
bash tools/generate-openapi.sh
```

#### What Happens During Code Generation

1. Builds the API project
2. Starts a temporary API instance
3. Fetches the OpenAPI spec from `/openapi/v1.json`
4. Generates TypeScript client using NSwag (`codegen/nswag.json` config)
5. Applies Angular compatibility fixes via `codegen/fix-api-client.js`
6. Outputs client to `src/app/api/api-client.ts`

**Note:** Code generation is manual. Run `npm run codegen` after modifying API contracts to keep the frontend in sync.

---

### Running Tests

#### Backend Tests

```bash
# From solution root
dotnet test

# From specific test project
cd src/QuokkaPack.Tests
dotnet test
```

#### Frontend Tests

```bash
cd src/QuokkaPack.Angular

# Run tests once
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with code coverage
npm test -- --code-coverage
```

---

### Development Tips

#### Hot Reload

- **API:** Supports hot reload with `dotnet watch run`
- **Angular:** Automatically reloads on file changes with `npm start`

#### Debugging

**API (Visual Studio):**
1. Open `QuokkaPack.sln`
2. Set `QuokkaPack.API` as startup project
3. Press F5 to debug

**API (VS Code):**
1. Open the project folder
2. Use the pre-configured launch configurations
3. Press F5 to debug

**Angular (VS Code):**
1. Install "Debugger for Chrome" or "Debugger for Microsoft Edge" extension
2. Use browser dev tools for debugging
3. Source maps are enabled in development mode

#### Multi-Project Launch

For Visual Studio users, you can launch both API and Angular simultaneously:

1. Right-click solution → "Set Startup Projects"
2. Select "Multiple startup projects"
3. Set `QuokkaPack.API` to "Start"
4. Set `QuokkaPack.Angular` to "Start" (requires custom launch configuration)

---

## Docker Deployment

### Local Docker Development

Build and run the entire stack with Docker Compose:

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes (cleans database)
docker-compose down -v
```

**Services:**
- **API:** `http://localhost:7100`
- **Razor App:** `http://localhost:7200` (legacy)
- **SQL Server:** `localhost:1433`

**Database Connection:**
- **Server:** `localhost,1433`
- **User:** `sa`
- **Password:** `YourStrongPassword123!` (change in `docker-compose.yml`)

### Apply Migrations to Docker Database

```bash
# Connect to the running API container and run migrations
docker-compose exec quokkapack.api dotnet ef database update
```

---

## Production Build

### Build the API

```bash
cd src/QuokkaPack.API
dotnet publish -c Release -o out
```

Output will be in `src/QuokkaPack.API/out/`.

### Build the Angular App

```bash
cd src/QuokkaPack.Angular
npm run build
```

Production output will be in `dist/QuokkaPack.Angular/browser/`.

**Serve the Angular build:**

The production build is a static site. Serve it with:
- **Nginx**
- **Apache**
- **IIS**
- **Azure Static Web Apps**
- **Netlify**
- **Vercel**

**Important:** Configure your web server to route all requests to `index.html` for Angular routing to work correctly.

---

## Production Deployment Checklist

Before deploying to production, ensure you've configured:

### 1. Secrets Management

- [ ] Set `JwtSettings:Secret` via environment variables (minimum 32 characters)
- [ ] Use secure secret management (Azure Key Vault, AWS Secrets Manager, etc.)
- [ ] Never commit secrets to source control
- [ ] Rotate secrets regularly

### 2. Database Configuration

- [ ] Configure production connection string via environment variables
- [ ] Use managed database service (Azure SQL, AWS RDS, etc.)
- [ ] Apply migrations: `dotnet ef database update`
- [ ] Enable automated backups
- [ ] Configure connection pooling

### 3. CORS Configuration

- [ ] Set `AllowedOrigins:Production` in `appsettings.Production.json` or environment variable
- [ ] Example: `AllowedOrigins__Production=https://quokkapack.yourdomain.com`
- [ ] Remove localhost origins in production
- [ ] Use HTTPS only in production

### 4. Rate Limiting

- [ ] Review and adjust rate limits in `appsettings.Production.json`
- [ ] Consider using Redis for distributed rate limiting across multiple instances
- [ ] Monitor rate limit violations

### 5. Logging & Monitoring

- [ ] Configure Serilog production sinks (Application Insights, CloudWatch, Datadog, etc.)
- [ ] Set appropriate log levels (Warning/Error in production)
- [ ] Set up alerts for errors and performance issues
- [ ] Monitor application health endpoints

### 6. Security

- [ ] Enable HTTPS everywhere
- [ ] Configure HSTS (HTTP Strict Transport Security)
- [ ] Review CORS policies
- [ ] Enable authentication on all protected endpoints
- [ ] Review rate limiting configuration
- [ ] Keep dependencies up to date

### 7. Performance

- [ ] Enable response caching where appropriate
- [ ] Configure CDN for static assets
- [ ] Enable gzip/brotli compression
- [ ] Optimize database queries and indexes
- [ ] Load test the application

---

## Environment Variables Reference

### API Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `JwtSettings__Secret` | JWT signing secret (min 32 chars) | `your-secret-key-here` |
| `ConnectionStrings__DefaultConnection` | Database connection string | `Server=...;Database=QuokkaPackDb;...` |
| `AllowedOrigins__Production` | CORS allowed origins | `https://quokkapack.com` |
| `Serilog__MinimumLevel__Default` | Minimum log level | `Warning` |

### Angular Environment Variables (Build Time)

Angular uses environment files for configuration:
- `src/environments/environment.ts` - Development
- `src/environments/environment.prod.ts` - Production

Modify these files to configure API URLs, feature flags, etc.

---

## Troubleshooting

### Database Connection Issues

**Problem:** Cannot connect to LocalDB

**Solutions:**
- Verify LocalDB is installed: `sqllocaldb info`
- Start LocalDB instance: `sqllocaldb start MSSQLLocalDB`
- Check connection string in `appsettings.json`
- Ensure migrations are applied: `dotnet ef database update`

### API Port Already in Use

**Problem:** Port 7100 or 5000 is already in use

**Solutions:**
- Find and kill the process using the port (Windows): `netstat -ano | findstr :7100`
- Change the port in `launchSettings.json` (QuokkaPack.API/Properties/)
- Update the Angular proxy configuration accordingly (`proxy.conf.json`)

### Code Generation Fails

**Problem:** `npm run codegen` fails

**Solutions:**
- Ensure the API can build: `dotnet build src/QuokkaPack.API`
- Verify no other instance of the API is running
- Check PowerShell/Bash is installed and in PATH
- Run the script manually: `pwsh tools/generate-openapi.ps1`
- Check the script output for specific errors

### JWT Authentication Fails

**Problem:** Login returns 401 Unauthorized

**Solutions:**
- Verify JWT secret is configured (user secrets or environment variable)
- Secret must be at least 32 characters
- Check API logs for authentication errors
- Ensure the database is seeded with a user or register a new user

### Angular Proxy Not Working

**Problem:** API requests fail with CORS errors

**Solutions:**
- Verify `proxy.conf.json` is configured correctly
- Ensure API is running on `https://localhost:7100`
- Check Angular dev server is using the proxy: `npm start`
- Review browser console for specific errors

---

## Common Commands Reference

### .NET Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API
dotnet run --project src/QuokkaPack.API

# Run with hot reload
dotnet watch run --project src/QuokkaPack.API

# Run tests
dotnet test

# Create migration
dotnet ef migrations add MigrationName --project src/QuokkaPack.Data --startup-project src/QuokkaPack.API

# Apply migrations
dotnet ef database update --project src/QuokkaPack.Data --startup-project src/QuokkaPack.API

# Install EF Core tools (if needed)
dotnet tool install --global dotnet-ef
```

### npm Commands

```bash
# Install dependencies
npm install

# Start dev server
npm start

# Build for production
npm run build

# Run tests
npm test

# Regenerate API client
npm run codegen

# Lint code
npm run lint
```

### Docker Commands

```bash
# Build and start services
docker-compose up -d

# View logs
docker-compose logs -f quokkapack.api

# Stop services
docker-compose down

# Rebuild containers
docker-compose up -d --build

# Execute command in container
docker-compose exec quokkapack.api bash
```

---

## Additional Resources

- **[Architecture Guide](ARCHITECTURE.md)** - Technical architecture and system design
- **[ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)**
- **[Angular Documentation](https://angular.dev/)**
- **[Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)**
- **[Docker Documentation](https://docs.docker.com/)**

---

**Need help?** Open an issue on [GitHub](https://github.com/PavelGutin/QuokkaPack/issues) or contribute to the project!

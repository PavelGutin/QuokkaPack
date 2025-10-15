# 🏗️ QuokkaPack Architecture

This document describes the technical architecture, system design, and API structure of QuokkaPack.

---

## Technology Stack

### Backend
- **.NET 9** - Modern C# web framework
- **ASP.NET Core Web API** - RESTful API with OpenAPI/Swagger documentation
- **Entity Framework Core** - ORM for database access
- **SQL Server / LocalDB** - Relational database
- **ASP.NET Core Identity** - User authentication and authorization
- **JWT Authentication** - Secure token-based authentication
- **Serilog** - Structured logging to console and file
- **AspNetCoreRateLimit** - API rate limiting

### Frontend
- **Angular 20** - Modern TypeScript framework with standalone components
- **Signals** - Reactive state management
- **RxJS** - Reactive programming with observables
- **Bootstrap 5.3** - Responsive UI framework
- **Bootswatch Sandstone** - Clean, professional theme
- **TypeScript 5.8** - Type-safe JavaScript
- **Auto-generated API Client** - TypeScript client generated from OpenAPI spec via NSwag

### Development Tools
- **Docker & Docker Compose** - Containerized deployment
- **NSwag** - OpenAPI client code generation
- **PowerShell & Bash Scripts** - Cross-platform tooling
- **EF Core Migrations** - Database schema versioning

---

## System Architecture

QuokkaPack follows a clean, API-first architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                      Angular 20 SPA                         │
│  (Bootstrap UI, Signals, Auto-generated API Client)         │
└──────────────────────┬──────────────────────────────────────┘
                       │ HTTPS (JWT Auth)
                       │ Proxy: localhost:4200 → localhost:7100
                       ↓
┌─────────────────────────────────────────────────────────────┐
│                   ASP.NET Core Web API                      │
│         (Controllers, JWT Auth, Rate Limiting)              │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────────┐
│              Entity Framework Core (ORM)                    │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────────┐
│            SQL Server / LocalDB Database                    │
│  (Identity Tables, Trips, Categories, Items, TripItems)     │
└─────────────────────────────────────────────────────────────┘
```

---

## Backend Architecture

### Project Structure

| Project | Description |
|---------|-------------|
| **QuokkaPack.API** | ASP.NET Core Web API with JWT auth, Swagger/OpenAPI docs, and RESTful endpoints |
| **QuokkaPack.Data** | Entity Framework Core data layer with SQL Server support and migrations |
| **QuokkaPack.Shared** | Shared models, DTOs, and mappings used across all projects |
| **QuokkaPack.ServerCommon** | Common server utilities, authentication setup, and middleware |

### Key Design Patterns

- **Repository Pattern** - Data access abstraction via EF Core DbContext
- **DTO Pattern** - Data Transfer Objects for API contracts separate from entity models
- **Dependency Injection** - Built-in ASP.NET Core DI container
- **Middleware Pipeline** - Authentication, rate limiting, CORS, error handling

---

## Frontend Architecture

### Angular Application Structure

```
src/app/
├── api/                    # Auto-generated API client (NSwag)
├── core/                   # Core services and guards
│   ├── auth.service.ts     # JWT authentication logic
│   ├── auth.guard.ts       # Route protection
│   └── api.service.ts      # API client wrapper
├── home/                   # Home/landing page
├── login/                  # Login component
├── trips/                  # Trip management features
└── items/                  # Item and category management
```

### Key Technologies

| Feature | Technology |
|---------|-----------|
| **Framework** | Angular 20 with standalone components and modern signals |
| **Routing** | Angular Router with lazy-loaded routes and auth guards |
| **HTTP** | Auto-generated TypeScript API client from OpenAPI spec |
| **State** | Reactive signals and RxJS observables |
| **Styling** | Bootstrap 5.3 + Bootswatch Sandstone theme |

### API Client Generation

The Angular app uses an auto-generated TypeScript client:

1. API builds and generates OpenAPI spec at `/openapi/v1.json`
2. NSwag fetches the spec and generates TypeScript client code
3. `fix-api-client.js` script applies Angular-specific fixes
4. Generated client provides fully typed API methods

**Proxy Configuration:**
- Development: Angular dev server (`:4200`) proxies `/api/*` to API (`:7100`)
- Production: Nginx or similar reverse proxy handles routing

---

## Database Schema

### Entity Relationship Diagram

```
MasterUser (ASP.NET Identity User)
    │
    ├──── Categories (1:Many)
    │         └──── Items (1:Many)
    │
    └──── Trips (1:Many)
              └──── TripItems (Many:Many with Items)
```

### Core Entities

#### **MasterUser**
Primary user entity linking to ASP.NET Identity. Owns all categories, items, and trips.

- **Properties**: `Id`, `Email`, `Username`
- **Relationships**: `Categories`, `Items`, `Trips`, `Logins`

#### **Trip**
Represents a travel trip with destination and date tracking.

- **Properties**: `Id`, `Destination`, `StartDate`, `EndDate`, `MasterUserId`, `IsDeleted`
- **Relationships**: `MasterUser`, `TripItems`

#### **Category**
User-defined grouping for items (e.g., "Hiking Gear", "Toiletries").

- **Properties**: `Id`, `Name`, `MasterUserId`, `IsArchived`
- **Relationships**: `MasterUser`, `Items`

#### **Item**
Individual packable item in user's catalog.

- **Properties**: `Id`, `Name`, `CategoryId`, `MasterUserId`, `IsArchived`
- **Relationships**: `Category`, `MasterUser`, `TripItems`

#### **TripItem**
Join table linking Trip and Item with packing status.

- **Properties**: `Id`, `TripId`, `ItemId`, `IsPacked`
- **Relationships**: `Trip`, `Item`
- **Purpose**: Enables same item in multiple trips with independent packing status

### Delete Behavior

- **Soft Delete**: Trips use `IsDeleted` flag (can be restored)
- **Archive**: Categories and Items use `IsArchived` flag
- **Cascade Prevention**: All foreign keys use `DeleteBehavior.Restrict` to prevent accidental data loss
- **Archive Blocking**: Categories with archived items cannot be deleted; items in archived categories cannot be deleted

---

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login with username/password (returns JWT token)
- `POST /api/auth/register` - Register new user

### Health Check
- `GET /api/health` - Basic health check (returns status and version)
- `GET /api/health/detailed` - Detailed health check (includes database connectivity)

### Trips
- `GET /api/trips` - List all trips for current user
- `GET /api/trips/{id}` - Get trip details with items
- `POST /api/trips` - Create new trip
- `PUT /api/trips/{id}` - Update trip
- `DELETE /api/trips/{id}` - Soft delete trip

### Trip Items
- `GET /api/trips/{tripId}/items` - List items in trip
- `POST /api/trips/{tripId}/items` - Add item to trip
- `PUT /api/trips/{tripId}/items/{id}` - Update trip item (e.g., mark as packed)
- `DELETE /api/trips/{tripId}/items/{id}` - Remove item from trip

### Categories
- `GET /api/categories` - List all categories for user (excludes archived by default)
- `POST /api/categories` - Create category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category (fails if has items)

### Items
- `GET /api/items` - List all items in catalog (excludes archived by default)
- `GET /api/items/category/{categoryId}` - List items by category
- `POST /api/items` - Create item
- `PUT /api/items/{id}` - Update item
- `DELETE /api/items/{id}` - Delete item (fails if in archived category)

### Trip Categories
- `POST /api/trips/{tripId}/categories/{categoryId}` - Add entire category to trip

**Full API documentation:** Available at `/swagger` when running in development mode.

---

## Security & Authentication

### JWT-based Authentication

- **Token Generation**: API issues JWT tokens on successful login
- **Token Storage**: Frontend stores token in memory (service)
- **Authorization Header**: `Authorization: Bearer {token}` on all authenticated requests
- **Token Claims**: User ID, email, expiration
- **Secret Management**: JWT secret stored in user secrets (dev) or environment variables (prod)

### User Isolation

All data is scoped to the authenticated user:
- Controllers validate `MasterUserId` from JWT claims
- Database queries filter by `MasterUserId`
- Users cannot access other users' data

### Rate Limiting

API rate limiting prevents abuse:
- **Auth endpoints** (`/api/auth/*`): 10 requests/minute
- **General endpoints**: 200 requests/minute
- Returns `429 Too Many Requests` when exceeded

### CORS Configuration

- **Development**: `localhost:4200`, `localhost:7100` allowed
- **Production**: Configure `AllowedOrigins:Production` in appsettings

---

## DevEx Features

### 🔄 Automatic API Client Generation

The TypeScript API client is auto-generated from the OpenAPI specification:

1. **API Build** → OpenAPI spec generated at `/openapi/v1.json`
2. **Script Execution** → `generate-openapi.ps1` or `.sh` starts API, fetches spec
3. **NSwag Generation** → Creates TypeScript client with full type safety
4. **Post-processing** → `fix-api-client.js` applies Angular compatibility fixes

**Run manually:**
```bash
cd src/QuokkaPack.Angular
npm run codegen
```

### 🛠️ Incremental Builds

Smart MSBuild targets only regenerate OpenAPI spec when DTOs or Controllers change, improving build performance.

### 📝 Type Safety

Full end-to-end TypeScript types from C# DTOs ensure compile-time safety and excellent IntelliSense support.

### 🔍 API Documentation

Swagger UI provides interactive API documentation at `/swagger` in development, including:
- Request/response schemas
- Try-it-out functionality
- Authentication configuration

---

## Deployment Architecture

### Docker Deployment

Docker Compose configuration includes:

- **API Container**: ASP.NET Core API on port 7100
- **Razor Container**: (Legacy) Razor Pages app on port 7200
- **SQL Server Container**: SQL Server 2022 with persistent volume
- **Environment Variables**: JWT secrets, connection strings via `.env` file

### Production Considerations

1. **Secrets Management**
   - Use Azure Key Vault, AWS Secrets Manager, or similar
   - Never commit secrets to source control
   - Set `JwtSettings:Secret` via environment variables (min 32 characters)

2. **Database**
   - Use managed SQL Server (Azure SQL, AWS RDS)
   - Configure connection string via environment variables
   - Apply migrations: `dotnet ef database update`

3. **CORS**
   - Configure `AllowedOrigins:Production` with your frontend domain
   - Remove localhost origins in production

4. **Rate Limiting**
   - Consider Redis for distributed rate limiting across multiple instances
   - Adjust limits based on expected traffic

5. **Logging**
   - Configure Serilog sinks for production (Application Insights, CloudWatch, etc.)
   - Set appropriate log levels (Warning/Error in production)

---

## Performance Considerations

- **Database Indexing**: Foreign keys and user IDs are indexed for efficient queries
- **EF Core Optimization**: Use `.AsNoTracking()` for read-only queries
- **API Caching**: Consider response caching for frequently accessed data
- **Angular Bundle Size**: Production build uses tree-shaking and minification
- **Lazy Loading**: Angular routes are lazy-loaded for faster initial load

---

**For setup and development instructions, see [Developer Guide](DEVELOPER_GUIDE.md)**

# 🧳 QuokkaPack

**QuokkaPack** is a smart, modern packing list application that helps you organize trips and never forget essential items. Built with .NET 9 and Angular 20, it features a clean API-first architecture with JWT authentication and a responsive single-page application interface.

Whether you're planning a weekend getaway, a family vacation, or a multi-week adventure, QuokkaPack makes trip preparation effortless with reusable categories, customizable item lists, and intuitive packing workflows.

---

## ✨ Features

### 🎯 Core Functionality

- **Trip Management**
  Create, edit, and delete trips with destination and date tracking. View all your trips in a responsive grid layout.

- **Smart Category System**
  Organize items into categories (e.g., "Clothes", "Electronics", "Toiletries"). Categories are user-specific and fully reusable across trips.

- **Flexible Item Management**
  Build your personal catalog of items tied to categories. Add, edit, or remove items from your master list.

- **Interactive Trip Packing**
  - **Pack Mode**: Check off items as you pack them with real-time status updates
  - **Edit Mode**: Add or remove items from your trip, manage categories
  - **Available Categories**: One-click add entire unused categories to your trip

- **Real-time Updates**
  Changes sync immediately with optimistic UI updates and automatic rollback on errors.

### 🔐 Security & Authentication

- **JWT-based Authentication**
  Secure token-based auth with automatic refresh and protected routes.

- **User Isolation**
  All data (trips, categories, items) is scoped to the authenticated user via `MasterUser` entity.

- **Auth Guards**
  Client-side route protection with automatic redirect to login for unauthenticated users.

- **Rate Limiting**
  API rate limiting to prevent abuse (10 auth requests/minute, 200 general requests/minute).

### 🎨 User Experience

- **Modern UI with Bootstrap**
  Clean, professional interface using Bootstrap 5.3 with Bootswatch Sandstone theme.

- **Responsive Design**
  Mobile-first layout that adapts to any screen size - works great on phones, tablets, and desktops.

- **Card-based Layouts**
  Intuitive organization with category cards, trip cards, and visual grouping.

- **Progressive Disclosure**
  Toggle between Pack and Edit modes without page reloads.

---

## 🏗️ Architecture

QuokkaPack follows a clean, API-first architecture with clear separation of concerns:

### Backend (.NET 9)

| Project | Description |
|---------|-------------|
| **QuokkaPack.API** | ASP.NET Core Web API with JWT auth, Swagger/OpenAPI docs, and RESTful endpoints |
| **QuokkaPack.Data** | Entity Framework Core data layer with SQL Server support and seeded test data |
| **QuokkaPack.Shared** | Shared models, DTOs, and mappings used across all projects |
| **QuokkaPack.ServerCommon** | Common server utilities, authentication setup, and middleware |

### Frontend (Angular 20)

| Feature | Technology |
|---------|-----------|
| **Framework** | Angular 20 with standalone components and modern signals |
| **Routing** | Angular Router with lazy-loaded routes and auth guards |
| **HTTP** | Auto-generated TypeScript API client from OpenAPI spec |
| **State** | Reactive signals and RxJS observables |
| **Styling** | Bootstrap 5.3 + Bootswatch Sandstone theme |

### DevEx Features

- **🔄 Automatic API Client Generation**
  OpenAPI spec → TypeScript client via NSwag on every API build

- **🛠️ Incremental Builds**
  Smart MSBuild targets only regenerate when DTOs/Controllers change

- **📝 Type Safety**
  Full end-to-end TypeScript types from C# DTOs

- **🔍 API Documentation**
  Swagger UI available at `/swagger` in development

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) or SQL Server instance
- [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell) (for codegen scripts)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/PavelGutin/QuokkaPack.git
   cd QuokkaPack
   ```

2. **Set up environment variables**
   ```bash
   # Copy the example .env file and configure your secrets
   cp .env.example .env
   # Edit .env and set JWT_SECRET to a secure random string (min 32 characters)
   ```

3. **Set up the database**
   ```bash
   cd src/QuokkaPack.API
   dotnet ef database update
   ```

4. **Configure JWT secrets**

   For **development**, use user secrets (recommended):
   ```bash
   cd src/QuokkaPack.API
   dotnet user-secrets set "JwtSettings:Secret" "your-super-secret-key-here-min-32-chars"
   ```

   For **production**, configure via environment variables or appsettings.Production.json (never commit secrets!)
   ```bash
   export JwtSettings__Secret="your-production-secret-min-32-chars"
   export ConnectionStrings__DefaultConnection="your-production-connection-string"
   ```

5. **Start the API**
   ```bash
   cd src/QuokkaPack.API
   dotnet run
   ```
   API will be available at `http://localhost:5000`

6. **Start the Angular app**
   ```bash
   cd src/QuokkaPack.Angular
   npm install
   npm start
   ```
   App will be available at `http://localhost:4200` (proxies to API on port 7100)

### First Time Setup

The database is automatically seeded with:
- Default categories (Clothes, Electronics, Toiletries, Documents, etc.)
- Sample items for each category
- A test user account

---

## 📋 API Endpoints

### Authentication
- `POST /api/auth/login` - Login with username/password
- `POST /api/auth/register` - Register new user

### Trips
- `GET /api/trips` - List all trips for current user
- `GET /api/trips/{id}` - Get trip details with items
- `POST /api/trips` - Create new trip
- `PUT /api/trips/{id}` - Update trip
- `DELETE /api/trips/{id}` - Delete trip

### Trip Items
- `GET /api/trips/{tripId}/items` - List items in trip
- `POST /api/trips/{tripId}/items` - Add item to trip
- `PUT /api/trips/{tripId}/items/{id}` - Update trip item (e.g., mark as packed)
- `DELETE /api/trips/{tripId}/items/{id}` - Remove item from trip

### Categories
- `GET /api/categories` - List all categories for user
- `POST /api/categories` - Create category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

### Items
- `GET /api/items` - List all items in catalog
- `GET /api/items/category/{categoryId}` - List items by category
- `POST /api/items` - Create item
- `PUT /api/items/{id}` - Update item
- `DELETE /api/items/{id}` - Delete item

Full API documentation available at `/swagger` when running in development mode.

---

## 🗄️ Database Schema

### Core Entities

**MasterUser**
Primary user entity linking to ASP.NET Identity. Owns all categories and items.

**Trip**
- `Destination`, `StartDate`, `EndDate`
- Belongs to `MasterUser`
- Contains multiple `TripItem`s

**Category**
- User-defined grouping (e.g., "Hiking Gear")
- Belongs to `MasterUser`
- Contains multiple `Item`s

**Item**
- Individual packable item in user's catalog
- Belongs to `Category` and `MasterUser`
- Can be added to multiple trips via `TripItem`

**TripItem**
- Join table linking `Trip` and `Item`
- Tracks `IsPacked` status per trip
- Enables same item in multiple trips with independent packing status

---

## 🛠️ Development Workflow

### Code Generation

TypeScript API clients are automatically generated when the backend changes:

```bash
# Manual generation (if needed)
cd src/QuokkaPack.Angular
npm run codegen
```

The generation process:
1. Builds the API project
2. Starts a temporary API instance
3. Fetches the OpenAPI spec from `/swagger/v1/swagger.json`
4. Generates TypeScript client using NSwag
5. Applies fixes for Angular compatibility

### Database Migrations

```bash
# Create a new migration
cd src/QuokkaPack.API
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Rollback to a specific migration
dotnet ef database update PreviousMigrationName
```

### Running Tests

```bash
# Backend tests
dotnet test

# Frontend tests
cd src/QuokkaPack.Angular
npm test
```

---

## 📦 Deployment

### Docker Support

Docker Compose configuration included for containerized deployment:

```bash
docker-compose up -d
```

This starts:
- ASP.NET Core API container
- SQL Server container
- Angular app served via nginx

### Production Build

**Important**: Before deploying to production, ensure you've configured:

1. **Secrets Management**
   - Set `JwtSettings:Secret` via environment variables (min 32 characters)
   - Never commit secrets to source control
   - Use Azure Key Vault, AWS Secrets Manager, or similar for production

2. **Database Connection**
   - Configure production connection string via environment variables
   - Update `appsettings.Production.json` (template included)

3. **CORS Configuration**
   - Update `Program.cs` CORS policy with your production domain
   - Remove localhost origins for production builds

4. **Rate Limiting**
   - Review and adjust rate limits in `appsettings.Production.json`
   - Consider using Redis for distributed rate limiting

**Build Commands:**

```bash
# Build API for production
dotnet publish src/QuokkaPack.API -c Release -o out

# Build Angular app for production
cd src/QuokkaPack.Angular
npm run build
# Output will be in dist/QuokkaPack.Angular/browser
```

**Environment Variables for Production:**

```bash
ASPNETCORE_ENVIRONMENT=Production
JwtSettings__Secret=your-production-jwt-secret
ConnectionStrings__DefaultConnection=your-production-db-connection
```

---

## 🔜 Roadmap

- [ ] **Sharing & Collaboration**
  Share trip packing lists with other users

- [ ] **Templates**
  Save and reuse trip configurations as templates

- [ ] **Duplicate Trips**
  Clone existing trips with all items

- [ ] **Advanced Filtering**
  Search and filter items across categories

- [ ] **Packing Statistics**
  Track packing completion percentage and history

- [ ] **Mobile Apps**
  Native iOS and Android apps

- [ ] **Offline Support**
  Progressive Web App with offline-first capabilities

- [ ] **Import/Export**
  CSV/JSON import and export for bulk operations

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is open source and available for personal and educational use.

---

## 🐨 Why "QuokkaPack"?

Quokkas are known as the happiest animals on earth - always smiling and ready for adventure! Just like QuokkaPack helps you prepare for your next adventure with a smile. 🦘✨

---

**Built with ❤️ using .NET 9, Angular 20, and modern web technologies**

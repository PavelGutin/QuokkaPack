# API Code Generation Setup

This document explains how the Angular API client is automatically generated from the .NET API project.

## How It Works

The project uses **NSwag** to generate a fully-typed TypeScript client from the QuokkaPack.API OpenAPI specification.

### Two-Step Process:

1. **Generate OpenAPI Spec** (`npm run codegen:spec`)
   - Builds the API project
   - Temporarily starts the API server
   - Fetches the OpenAPI/Swagger JSON from `http://localhost:5000/swagger/v1/swagger.json`
   - Saves to `artifacts/openapi.json`
   - Stops the API server

2. **Generate TypeScript Client** (`npm run codegen:client`)
   - Reads `artifacts/openapi.json`
   - Generates Angular HttpClient-based services
   - Outputs to `src/app/api/api-client.ts`

## Developer Workflow

### First-Time Setup

```bash
# From repository root
git clone <repository>
cd QuokkaPack

# Restore .NET tools (includes NSwag)
dotnet tool restore

# Install Angular dependencies
cd src/QuokkaPack.Angular
npm install
```

### Daily Development

The API client is **automatically regenerated** when you:

```bash
npm start    # Runs codegen before starting dev server
npm run build   # Runs codegen before building
```

### Manual Regeneration

If you modify API controllers or DTOs:

```bash
npm run codegen
```

Or run steps individually:

```bash
npm run codegen:spec     # Step 1: Generate OpenAPI spec
npm run codegen:client   # Step 2: Generate TypeScript client
```

## Requirements

- **.NET SDK 9.0** (for building the API)
- **PowerShell** (for the codegen script)
- **NSwag** (installed via `dotnet tool restore`)

## Generated Files

### Auto-Generated (Don't Edit):
- `src/app/api/api-client.ts` - Full Angular client with all API methods
- `artifacts/openapi.json` - OpenAPI specification

Both files are in `.gitignore` and regenerated on build.

## Troubleshooting

### "Failed to fetch OpenAPI spec"
- Ensure no other process is using port 5000
- Check if the API builds successfully: `dotnet build ../../src/QuokkaPack.API`

### "NSwag command not found"
- Run `dotnet tool restore` from repository root

### Generated client is outdated
- Delete `artifacts/openapi.json` and `src/app/api/api-client.ts`
- Run `npm run codegen`

## What Gets Generated

### **Primary Output:**
**`src/app/api/api-client.ts`** (~116KB)
- **Client class** - Injectable Angular service with all API methods
- **DTO Classes** - TypeScript classes for instantiation (e.g., `new TripCreateDto()`)
- **DTO Interfaces** - TypeScript interfaces for type-checking (e.g., `ITripCreateDto`)
- **API_BASE_URL** - Injection token for configuring the API endpoint
- **Full type safety** - Compile-time checking for all API calls

### **Type Re-exports:**
**`src/app/core/models/api-types.ts`** (maintained manually)
- Re-exports commonly used types from `api-client.ts`
- Provides a stable import path for components
- Allows custom view model extensions

### Example Usage:

**Using types from api-types.ts (recommended):**
```typescript
import { TripCreateDto, CategoryReadDto } from '../core/models/api-types';

@Component({ ... })
export class TripsComponent {
  createTrip(dto: TripCreateDto) { ... }
}
```

**Using the generated client directly:**
```typescript
import { Client } from '../api/api-client';

@Injectable()
export class TripService {
  constructor(private apiClient: Client) {}

  createTrip(dto: TripCreateDto) {
    return this.apiClient.tripsPOST(dto);
  }
}
```

## Build-Time Workflow

```
npm start / npm build
      ↓
  (prestart/prebuild hook)
      ↓
  npm run codegen
      ↓
┌─────────────────────────────┐
│  codegen:spec               │
│  1. Build API (Release)     │
│  2. Start API temporarily   │
│  3. Fetch swagger.json      │
│  4. Save to artifacts/      │
│  5. Stop API                │
└──────────┬──────────────────┘
           ↓
┌─────────────────────────────┐
│  codegen:client             │
│  1. Read artifacts/openapi.json │
│  2. Generate TypeScript     │
│  3. Save to src/app/api/    │
└─────────────────────────────┘
```

## Notes

- The API must be successfully built before codegen can run
- Codegen adds ~10-15 seconds to the first `npm start`
- Generated client matches API exactly (no drift)
- Industry-standard approach for .NET + Angular projects

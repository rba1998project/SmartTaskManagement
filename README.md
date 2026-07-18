# Smart Task Management System

A role-based task and project management system built as a 4-day interview assignment.
Backend: ASP.NET Core 9 (Clean Architecture) with EF Core 9 and SQL Server. Frontend:
Angular 21+ (added in a later phase).

> **Status:** Phase 1 (architecture & project foundation) complete — the API runs and the
> database connects through the initial EF Core migration. Auth, projects, tasks, dashboard,
> the AI feature, and the Angular client are delivered in later phases. See `PLAN.md`.

## Technology Stack

| Layer | Technology |
|-------|------------|
| API / host | ASP.NET Core 9 |
| Application | .NET 9 class library (business rules, DTOs, service abstractions) |
| Domain | .NET 9 class library (entities, enums) |
| Persistence | EF Core 9 + SQL Server (LocalDB for local dev) |
| Logging | Serilog (console + rolling file) |
| API docs | Swashbuckle / Swagger (Development only) |
| Frontend | Angular 21+ (later phase) |

## Folder Structure

```
SmartTaskManagement/
├─ SmartTaskManagement.sln
├─ CLAUDE.md                        # project instructions / constraints
├─ PLAN.md                          # living implementation plan
├─ README.md
├─ SmartTaskManagement.Domain/          # entities, enums (depends on nothing)
├─ SmartTaskManagement.Application/     # business rules, DTOs, abstractions → Domain
├─ SmartTaskManagement.Infrastructure/  # EF Core, DbContext, migrations → Application/Domain
│  ├─ Persistence/ApplicationDbContext.cs
│  ├─ Migrations/
│  └─ DependencyInjection.cs            # AddInfrastructure(IConfiguration)
├─ SmartTaskManagement.API/             # ASP.NET Core host → Application/Infrastructure
│  ├─ Common/ApiResponse.cs             # consistent response envelope
│  ├─ Extensions/                       # focused DI/pipeline composition
│  ├─ Middleware/ExceptionHandlingMiddleware.cs
│  └─ Program.cs                        # composition root
└─ client/smart-task-ui/                # Angular app (later phase)
```

**Dependency direction:** `API → Application → Domain` and `Infrastructure → Application/Domain`.
Inner layers never reference outer layers.

## Prerequisites

- .NET 9 SDK
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) — or adjust the connection string for your server
- EF Core tools: `dotnet tool install --global dotnet-ef`

## Setup

1. **Restore & build**
   ```bash
   dotnet build
   ```

2. **Configure the connection string** (via User Secrets — never committed):
   ```bash
   dotnet user-secrets set "ConnectionStrings:SmartTaskConnection" "Server=(localdb)\MSSQLLocalDB;Database=SmartTaskManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True" --project SmartTaskManagement.API
   ```
   > The key is `SmartTaskConnection` (not `DefaultConnection`) to avoid colliding with an
   > unrelated machine-level environment variable. `appsettings.json` intentionally holds no
   > secrets.

3. **Apply migrations** (creates `SmartTaskManagementDb`):
   ```bash
   dotnet ef database update --project SmartTaskManagement.Infrastructure --startup-project SmartTaskManagement.API
   ```

4. **Run the API**
   ```bash
   dotnet run --project SmartTaskManagement.API
   ```
   - HTTP: `http://localhost:5193`
   - HTTPS: `https://localhost:7277`

## API Overview (Phase 1)

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Health check; returns an `ApiResponse` envelope with the DB check status |
| `GET /swagger` | Swagger UI (Development only) |
| `GET /swagger/v1/swagger.json` | OpenAPI document (Development only) |

Feature endpoints (auth, projects, tasks, dashboard, AI) are added in later phases.

### Cross-cutting behavior
- **Consistent responses:** every response uses the `ApiResponse` / `ApiResponse<T>` envelope
  (`success`, `message`, `data`, `errors`).
- **Global exception handling:** unhandled exceptions are logged and returned as a consistent
  error envelope (no stack traces leaked; details shown only in Development).
- **Logging:** Serilog request logging to console and a rolling daily file (`logs/log-*.txt`).
- **CORS:** named policy `AngularDevClient`, restricted to origins in `Cors:AllowedOrigins`
  (default `http://localhost:4200`).
- **Rate limiting:** fixed-window global limiter, 100 requests/minute per client IP (429 on
  rejection).
- **HTTPS redirection** enabled.

## Commands

```bash
dotnet build     # build the solution
dotnet test      # run tests (test project added in a later phase)
dotnet run --project SmartTaskManagement.API   # run the API

# EF Core
dotnet ef migrations add <Name> --project SmartTaskManagement.Infrastructure --startup-project SmartTaskManagement.API
dotnet ef database update      --project SmartTaskManagement.Infrastructure --startup-project SmartTaskManagement.API
```

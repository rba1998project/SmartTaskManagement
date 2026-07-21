# Smart Task Management System

A role-based task and project management system built as a 4-day interview assignment.
Backend: ASP.NET Core 9 (Clean Architecture) with EF Core 9 and SQL Server. Frontend:
Angular 21+ (added in a later phase).

> **Status:** Backend feature work is in progress. Authentication (JWT + rotating refresh
> tokens), project management, task management, and permission-based (RBAC) authorization are
> complete. Search/filtering/sorting/pagination, the dashboard, the AI task-description
> improver, and the Angular client are delivered in later phases. See `PLAN.md`.

## Technology Stack

| Layer | Technology |
|-------|------------|
| API / host | ASP.NET Core 9 |
| Application | .NET 9 class library (business rules, DTOs, service abstractions) |
| Domain | .NET 9 class library (entities, enums) |
| Persistence | EF Core 9 + SQL Server (LocalDB for local dev) |
| Identity & auth | ASP.NET Core Identity, JWT bearer, refresh tokens |
| Validation | FluentValidation |
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
│  └─ Entities/                         # RefreshToken, Project, TaskItem, status/priority enums
├─ SmartTaskManagement.Application/     # business rules, DTOs, abstractions → Domain
│  ├─ Abstractions/                     # IProjectRepository, ITaskRepository, IIdentityService,
│  │                                    #   IJwtTokenGenerator, IRefreshTokenService, ICurrentUserService
│  ├─ Authentication/                   # auth DTOs, validators, AuthService
│  ├─ Authorization/                    # RoleNames, Permissions (role→permission map)
│  ├─ Projects/                         # DTOs, validators, ProjectService
│  ├─ Tasks/                            # DTOs, validators, TaskService
│  └─ Common/                           # Result / ErrorType
├─ SmartTaskManagement.Infrastructure/  # EF Core, DbContext, migrations → Application/Domain
│  ├─ Identity/                         # ApplicationUser/Role, IdentityDataSeeder, IdentityService
│  ├─ Authentication/                   # JwtTokenGenerator, RefreshTokenService, JwtOptions
│  ├─ Persistence/                      # ApplicationDbContext, repositories, EF configurations
│  ├─ Migrations/
│  └─ DependencyInjection.cs            # AddInfrastructure(IConfiguration)
├─ SmartTaskManagement.API/             # ASP.NET Core host → Application/Infrastructure
│  ├─ Controllers/                      # AuthController, ProjectsController, TasksController
│  ├─ Common/                           # ApiResponse envelope, Result→ActionResult mapping
│  ├─ Extensions/                       # focused DI/pipeline composition
│  ├─ Filters/ValidationActionFilter.cs # model validation → consistent error envelope
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

2. **Configure secrets** (via User Secrets — never committed). The connection string, the JWT
   signing key, and the seeded admin password all live here:
   ```bash
   dotnet user-secrets set "ConnectionStrings:SmartTaskConnection" "Server=(localdb)\MSSQLLocalDB;Database=SmartTaskManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True" --project SmartTaskManagement.API

   # 64-byte (base64) random key used to sign JWTs
   dotnet user-secrets set "Jwt:SigningKey" "<base64-encoded-random-key>" --project SmartTaskManagement.API

   # password for the seeded admin user (email is in appsettings under Seed:AdminEmail)
   dotnet user-secrets set "Seed:AdminPassword" "<strong-password>" --project SmartTaskManagement.API
   ```
   > The connection-string key is `SmartTaskConnection` (not `DefaultConnection`) to avoid
   > colliding with an unrelated machine-level environment variable. `appsettings.json`
   > intentionally holds no secrets — only non-secret config (`Jwt` issuer/audience/lifetimes,
   > `Cors`, `Serilog`, `Seed:AdminEmail`).

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
   - On startup the app seeds the three roles and (if configured) a default admin user.

## Roles & Permissions

Authorization is **permission-based**. Each role is seeded with a set of `permission` claims;
those claims are carried in the JWT, and protected endpoints require a matching policy. Resource
ownership (a Project Manager may touch only their own projects; a Team Member may change status
only on tasks assigned to them) is enforced in the service layer, not in the token.

| Role | Feature permissions | Notes |
|------|--------------------|-------|
| Admin | all | Full access to every project and task. |
| Project Manager | all | Ownership narrows create/update/delete to their own projects/tasks. |
| Team Member | none | Reads assigned work and changes status on tasks assigned to them. |

Permissions in use: `projects.create`, `projects.update`, `projects.delete`, `tasks.create`,
`tasks.update`, `tasks.delete`, `tasks.assign`. Listing, viewing, and task status changes are
open to any authenticated user (with visibility/ownership rules applied in the services).

## API Overview

All responses use the `ApiResponse` / `ApiResponse<T>` envelope. Send the JWT as
`Authorization: Bearer <access token>` on authenticated endpoints.

### Auth — `api/auth`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `POST /api/auth/register` | anonymous | Create a user with the default `TeamMember` role (no tokens returned). |
| `POST /api/auth/login` | anonymous | Verify credentials; return access token + refresh token. |
| `POST /api/auth/refresh` | anonymous | Rotate the refresh token; return a new token pair. |
| `POST /api/auth/logout` | authenticated | Revoke the presented refresh token. |

Refresh tokens are persisted as SHA-256 hashes, rotated on every use, and revocable on logout.

### Projects — `api/projects`
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `GET /api/projects` | authenticated | List projects (Team Member sees only projects from assigned tasks). |
| `GET /api/projects/{id}` | authenticated | Project details. |
| `POST /api/projects` | `projects.create` | Create a project. |
| `PUT /api/projects/{id}` | `projects.update` | Update a project (ownership enforced). |
| `DELETE /api/projects/{id}` | `projects.delete` | Delete a project; cascades to its tasks. |

### Tasks
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `POST /api/projects/{projectId}/tasks` | `tasks.create` | Create a task in a project. |
| `GET /api/projects/{projectId}/tasks` | authenticated | List a project's tasks. |
| `GET /api/tasks/{id}` | authenticated | Task details. |
| `PUT /api/tasks/{id}` | `tasks.update` | Update task details. |
| `DELETE /api/tasks/{id}` | `tasks.delete` | Delete a task. |
| `PUT /api/tasks/{id}/assignment` | `tasks.assign` | Assign the task to a user. |
| `PUT /api/tasks/{id}/status` | authenticated | Change status (Team Member: only own assigned tasks). |

**Task status:** `ToDo`, `InProgress`, `Completed`, `Cancelled`.
**Task priority:** `Low`, `Medium`, `High`, `Critical`.

### Operational
| Endpoint | Description |
|----------|-------------|
| `GET /health` | Health check; returns an `ApiResponse` envelope with the DB check status. |
| `GET /swagger` | Swagger UI (Development only). |
| `GET /swagger/v1/swagger.json` | OpenAPI document (Development only). |

> Not yet implemented: server-side search/filtering/sorting/pagination on list endpoints, the
> dashboard statistics endpoint, and the AI task-description improver. See `PLAN.md`.

### Cross-cutting behavior
- **Consistent responses:** every response uses the `ApiResponse` / `ApiResponse<T>` envelope
  (`success`, `message`, `data`, `errors`).
- **Validation:** FluentValidation via a global `ValidationActionFilter`; invalid requests return
  the consistent error envelope.
- **Global exception handling:** unhandled exceptions are logged and returned as a consistent
  error envelope (no stack traces leaked; details shown only in Development).
- **Authentication/authorization:** JWT bearer with permission-based policies; ownership and
  visibility rules enforced in the service layer.
- **Logging:** Serilog request logging to console and a rolling daily file (`logs/log-*.txt`).
- **CORS:** named policy `AngularDevClient`, restricted to origins in `Cors:AllowedOrigins`
  (default `http://localhost:4200`).
- **Rate limiting:** fixed-window global limiter, 100 requests/minute per client IP (429 on
  rejection).
- **HTTPS redirection** enabled.

## Database & Migrations

Applied migrations:

| Migration | Contents |
|-----------|----------|
| `InitialCreate` | Empty baseline. |
| `AddIdentityAndRefreshTokens` | ASP.NET Core Identity tables + `RefreshTokens`. |
| `AddProjects` | `Projects` table. |
| `AddTasks` | `Tasks` table with `Project → Task` cascade delete. |

## Commands

```bash
dotnet build     # build the solution
dotnet test      # run tests (test project added in a later phase)
dotnet run --project SmartTaskManagement.API   # run the API

# EF Core
dotnet ef migrations add <Name> --project SmartTaskManagement.Infrastructure --startup-project SmartTaskManagement.API
dotnet ef database update      --project SmartTaskManagement.Infrastructure --startup-project SmartTaskManagement.API
```

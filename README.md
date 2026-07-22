# Smart Task Management System

A role-based task and project management system built as a 4-day interview assignment.
Backend: ASP.NET Core 9 (Clean Architecture) with EF Core 9 and SQL Server.
Frontend: Angular 21+ with Angular Material.

> **Status:** Backend and Angular frontend are both implemented. Authentication (JWT +
> rotating refresh tokens), project management, task management, permission-based (RBAC)
> authorization, search/filtering/sorting/pagination, dashboard, AI task-description
> improver, and responsive UI are all working. See `PLAN.md`.

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
| Frontend | Angular 21+, Angular Material, standalone components, signal-based state |

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
│  ├─ Authorization/                    # RoleNames, permissions
│  ├─ Projects/                         # DTOs, validators, ProjectService
│  ├─ Tasks/                            # DTOs, validators, TaskService
│  ├─ Dashboard/                        # DashboardService, DashboardResponse
│  └─ Common/                           # Result / ErrorType / PagedResult / SortDirection
├─ SmartTaskManagement.Infrastructure/  # EF Core, DbContext, migrations → Application/Domain
│  ├─ Identity/                         # ApplicationUser/Role, IdentityDataSeeder, IdentityService
│  ├─ Authentication/                   # JwtTokenGenerator, RefreshTokenService, JwtOptions
│  ├─ Persistence/                      # ApplicationDbContext, repositories, EF configurations
│  ├─ Ai/                               # GeminiTaskAiService, AiPrompts, AiStatusService
│  ├─ Migrations/
│  └─ DependencyInjection.cs            # AddInfrastructure(IConfiguration)
├─ SmartTaskManagement.API/             # ASP.NET Core host → Application/Infrastructure
│  ├─ Controllers/                      # AuthController, ProjectsController, TasksController, DashboardController, AiController
│  ├─ Common/                           # ApiResponse envelope, Result→ActionResult mapping
│  ├─ Extensions/                       # focused DI/pipeline composition
│  ├─ Filters/ValidationActionFilter.cs # model validation → consistent error envelope
│  ├─ Middleware/ExceptionHandlingMiddleware.cs
│  └─ Program.cs                        # composition root
└─ client/smart-task-ui/                # Angular 21+ frontend
   └─ src/app/
      ├─ app.config.ts
      ├─ app.routes.ts
      ├─ core/                           # auth, guards, interceptors, services, models
      ├─ layouts/shell/                  # sidenav, toolbar, shell
      ├─ features/                       # dashboard, projects, tasks, auth, account
      └─ shared/                         # components, constants, pipes
```

**Dependency direction:** `API → Application → Domain` and `Infrastructure → Application/Domain`.
Inner layers never reference outer layers.

## Prerequisites

- .NET 9 SDK
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) — or adjust the connection string for your server
- EF Core tools: `dotnet tool install --global dotnet-ef`
- Node.js 18+ and npm (for the Angular frontend)

## Setup

1. **Restore & build backend**
   ```bash
   dotnet build
   ```

2. **Configure secrets** (via User Secrets — never committed). The connection string, the JWT
   signing key, the seeded admin password, and the AI API key all live here:
   ```bash
   dotnet user-secrets set "ConnectionStrings:SmartTaskConnection" "Server=(localdb)\MSSQLLocalDB;Database=SmartTaskManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True" --project SmartTaskManagement.API

   # 64-byte (base64) random key used to sign JWTs
   dotnet user-secrets set "Jwt:SigningKey" "<base64-encoded-random-key>" --project SmartTaskManagement.API

   # password for the seeded admin user (email is admin@smarttask.local)
   # If this secret is not set, no admin user is created.
   dotnet user-secrets set "Seed:AdminPassword" "<strong-password>" --project SmartTaskManagement.API

   # AI provider API key (optional; enables POST /api/tasks/improve-description)
   dotnet user-secrets set "Ai:ApiKey" "<your-api-key>" --project SmartTaskManagement.API
   ```
   > The connection-string key is `SmartTaskConnection` (not `DefaultConnection`) to avoid
   > colliding with an unrelated machine-level environment variable. `appsettings.json`
   > intentionally holds no secrets — only non-secret config (`Jwt` issuer/audience/lifetimes,
   > `Cors`, `Serilog`, `Seed:AdminEmail`, `Ai` provider/model/timeout/header).

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

5. **Run the Angular frontend** (in a separate terminal)
   ```bash
   cd client/smart-task-ui
   npm install
   npm start
   ```
   - The frontend proxies API requests to `https://localhost:7277`.
   - Default login: `admin@smarttask.local` / the password set in User Secrets.

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
| `GET /api/projects` | authenticated | List projects with search, filtering, sorting, and pagination. |
| `GET /api/projects/{id}` | authenticated | Project details. |
| `POST /api/projects` | `projects.create` | Create a project. |
| `PUT /api/projects/{id}` | `projects.update` | Update a project (ownership enforced). |
| `DELETE /api/projects/{id}` | `projects.delete` | Delete a project; cascades to its tasks. |

**List query parameters:** `search` (keyword), `sortField` (`Name`, `CreatedAt`, `UpdatedAt`), `sortDirection` (`Asc`, `Desc`), `pageNumber`, `pageSize`.

### Tasks
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `POST /api/projects/{projectId}/tasks` | `tasks.create` | Create a task in a project. |
| `GET /api/projects/{projectId}/tasks` | authenticated | List a project's tasks with search, filtering, sorting, and pagination. |
| `GET /api/tasks/{id}` | authenticated | Task details. |
| `PUT /api/tasks/{id}` | `tasks.update` | Update task details. |
| `DELETE /api/tasks/{id}` | `tasks.delete` | Delete a task. |
| `PUT /api/tasks/{id}/assignment` | `tasks.assign` | Assign the task to a user. |
| `PUT /api/tasks/{id}/status` | authenticated | Change status (Team Member: only own assigned tasks). |
| `POST /api/tasks/improve-description` | authenticated | Improve a task description using the AI provider. |

**Task status:** `ToDo`, `InProgress`, `Completed`, `Cancelled`.
**Task priority:** `Low`, `Medium`, `High`, `Critical`.

**List query parameters:** `search` (keyword), `status`, `priority`, `dueDate` (on or before), `sortField` (`Title`, `CreatedAt`, `DueDate`, `Priority`, `Status`), `sortDirection` (`Asc`, `Desc`), `pageNumber`, `pageSize`.

### Dashboard
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `GET /api/dashboard` | authenticated | Basic statistics: total projects, total tasks, tasks by status, tasks by priority, upcoming due tasks. |

### AI
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `GET /api/ai/status` | anonymous | Returns whether the AI description improver is configured (`enabled`). |
| `POST /api/tasks/improve-description` | authenticated | Returns an improved task description (plain text). Requires `Ai:ApiKey` in User Secrets. |

### Operational
| Endpoint | Description |
|----------|-------------|
| `GET /health` | Health check; returns an `ApiResponse` envelope with the DB check status. |
| `GET /swagger` | Swagger UI (Development only). |
| `GET /swagger/v1/swagger.json` | OpenAPI document (Development only). |

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
- **HTTPS:** HTTPS redirection is enabled. For production, ensure TLS termination at your reverse proxy or app server.

## Database & Migrations

Applied migrations:

| Migration | Contents |
|-----------|----------|
| `InitialCreate` | Empty baseline. |
| `AddIdentityAndRefreshTokens` | ASP.NET Core Identity tables + `RefreshTokens`. |
| `AddProjects` | `Projects` table. |
| `AddTasks` | `Tasks` table with `Project → Task` cascade delete. |

## Frontend Architecture

The Angular frontend lives in `client/smart-task-ui/` and follows these conventions:

- **Standalone components** — no NgModules.
- **Angular Material + CDK** — `MatTable`, `MatPaginator`, `MatSort`, `MatDialog`, `MatSnackBar`, `BreakpointObserver`.
- **Signal-based state** — `signal()`, `computed()`, and reactive forms.
- **RxJS cleanup** — `takeUntilDestroyed()` from `@angular/core/rxjs-interop` on all subscriptions.
- **Route structure:**
  - `/login`, `/register` — auth pages
  - `/dashboard` — stats and recent projects/tasks
  - `/projects` — project list with search/sort/pagination
  - `/projects/create` — create project
  - `/projects/:id` — project detail
  - `/projects/:id/edit` — edit project
  - `/tasks` — task list with search/status/priority filters, sort/pagination
  - `/tasks/create` — create task
  - `/tasks/:id` — task detail
  - `/tasks/:id/edit` — edit task
  - `/account/profile` — current user profile
- **Guards:** `authGuard` redirects unauthenticated users to `/login`; `roleGuard` restricts by role.
- **Interceptors:** `authInterceptor` attaches JWT and handles refresh; `errorInterceptor` sanitizes error messages and maps HTTP status codes to user-friendly toasts.

## Commands

```bash
# Backend
dotnet build                                    # build the solution
dotnet run --project SmartTaskManagement.API    # run the API

# EF Core
dotnet ef migrations add <Name> --project SmartTaskManagement.Infrastructure --startup-project SmartTaskManagement.API
dotnet ef database update                       --project SmartTaskManagement.Infrastructure --startup-project SmartTaskManagement.API

# Frontend
cd client/smart-task-ui
npm install
npm start                                       # dev server with proxy to API
npm run build                                   # production build
```

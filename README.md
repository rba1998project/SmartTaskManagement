# Smart Task Management System

A role-based task and project management system built with ASP.NET Core 9 and Angular 21+.
It includes JWT authentication with refresh tokens, permission-based authorization, project
and task management with search/filtering/sorting/pagination, a dashboard with aggregate
statistics, soft delete, and an AI-powered task description improver.

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
├─ README.md
├─ SmartTaskManagement.Domain/          # entities, enums (depends on nothing)
│  └─ Entities/                         # RefreshToken, Project, TaskItem, status/priority enums
├─ SmartTaskManagement.Application/     # business rules, DTOs, abstractions → Domain
│  ├─ Abstractions/                     # IProjectRepository, ITaskRepository, IIdentityService,
│  │                                    #   IJwtTokenGenerator, IRefreshTokenService, ICurrentUserService
│  ├─ Authentication/                   # auth DTOs, validators, AuthService
│  ├─ Authorization/                    # RoleNames, permissions
│  ├─ Users/                            # user lookup DTOs, user management DTOs, validators
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
│  ├─ Authentication/                   # CurrentUserService
│  ├─ Controllers/                      # AuthController, ProjectsController, TasksController, DashboardController, AiController, UsersController
│  ├─ Common/                           # ApiResponse envelope, Result→ActionResult mapping
│  ├─ Extensions/                       # focused DI/pipeline composition
│  ├─ Filters/ValidationActionFilter.cs # model validation → consistent error envelope
│  ├─ Middleware/ExceptionHandlingMiddleware.cs
│  └─ Program.cs                        # composition root
└─ client/smart-task-ui/                # Angular 21+ frontend
    └─ src/app/
       ├─ app.config.ts
       ├─ app.routes.ts
       ├─ app.ts
       ├─ app.html
       ├─ core/                           # auth, guards, interceptors, services, models
       ├─ layouts/shell/                  # sidenav, toolbar, shell
       ├─ features/                       # dashboard, projects, tasks, auth, account, user-management
       ├─ error/                          # 403, 404 page components
       └─ shared/                         # components, constants
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

2. **Configure secrets** (never committed). For local development you can use .NET User Secrets;
   for production use environment variables. The connection string, the JWT signing key, the seeded
   admin password, and the AI API key all live here:
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
   >
   > **Production:** set these values as environment variables on the host instead of User Secrets.
   > ASP.NET Core reads `ConnectionStrings:SmartTaskConnection`, `Jwt:SigningKey`,
   > `Seed:AdminPassword`, and `Ai:ApiKey` from the environment automatically.

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
   - Swagger UI: `https://localhost:7277/swagger` (Development only)
   - On startup the app seeds the three roles and (if configured) a default admin user.

5. **Run the Angular frontend** (in a separate terminal)
   ```bash
   cd client/smart-task-ui
   npm install
   npm start
   ```
   - The frontend calls the API directly at `https://localhost:7277` (configured in `environment.ts`).
   - Default login: `admin@smarttask.local` / the password set in User Secrets.

## Roles & Permissions

Authorization is **permission-based**. Each role is seeded with a set of `permission` claims;
those claims are carried in the JWT, and protected endpoints require a matching policy. Resource
ownership is enforced in the service layer, not in the token:
- **Admin:** full access to every project, task, and user management.
- **Project Manager:** can view, create, update, and delete only their own projects and the tasks within them.
- **Team Member:** can view only projects/tasks assigned to them, and change status on tasks assigned to them.

| Role | Feature permissions | Notes |
|------|--------------------|-------|
| Admin | all | Full access to every project, task, and user management. |
| Project Manager | projects.*, tasks.* | Can view, create, update, and delete only their own projects and the tasks within them. Cannot manage users. |
| Team Member | none | Can view only projects/tasks assigned to them, and change status on tasks assigned to them. |

Permissions in use: `projects.create`, `projects.update`, `projects.delete`, `tasks.create`,
`tasks.update`, `tasks.delete`, `tasks.assign`, `users.manage`. Listing, viewing, and task status changes are
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
| `POST /api/auth/logout` | anonymous | Revoke the supplied refresh token so it can no longer be exchanged. |

Refresh tokens are persisted as SHA-256 hashes, rotated on every use, and revocable on logout.

### Projects — `api/projects`
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `GET /api/projects` | authenticated | List projects with search, filtering, sorting, and pagination. Visibility: Admin sees all; Project Manager sees only their own; Team Member sees only projects containing tasks assigned to them. |
| `GET /api/projects/{id}` | authenticated | Project details. Visibility rules same as list. |
| `POST /api/projects` | `projects.create` | Create a project. |
| `PUT /api/projects/{id}` | `projects.update` | Update a project (ownership enforced). |
| `DELETE /api/projects/{id}` | `projects.delete` | Soft-delete a project; cascades soft deletion to its tasks. |

**List query parameters:** `search` (keyword), `status` (enum), `priority` (enum), `dueDate` (date only),
`assignedToUserId` (guid), `sortField` (`Title`, `DueDate`, `Priority`, `Status`, `CreatedAt`),
`sortDirection` (`Asc`, `Desc`), `pageNumber`, `pageSize`.

**List response shape:**
```json
{
  "success": true,
  "data": {
    "items": [ /* ProjectResponse[] */ ],
    "totalCount": 10,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

**ProjectResponse:**
```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "name": "Project name",
  "description": "Optional description",
  "createdByUserId": "00000000-0000-0000-0000-000000000000",
  "createdByUserName": "Admin User",
  "createdAt": "2026-07-23T00:00:00Z",
  "updatedAt": "2026-07-23T00:00:00Z"
}
```

### Tasks
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `GET /api/tasks` | authenticated | Global task list with search, filtering, sorting, and pagination. Visibility: Admin sees all; Project Manager sees only tasks in their own projects; Team Member sees only tasks assigned to them. |
| `GET /api/projects/{projectId}/tasks` | authenticated | List a single project's tasks. Visibility rules same as global task list. |
| `GET /api/tasks/{id}` | authenticated | Task details. Visibility rules same as global task list. |
| `POST /api/projects/{projectId}/tasks` | `tasks.create` | Create a task in a project. |
| `PUT /api/tasks/{id}` | `tasks.update` | Update task details. |
| `DELETE /api/tasks/{id}` | `tasks.delete` | Soft-delete a task. |
| `PUT /api/tasks/{id}/assignment` | `tasks.assign` | Assign the task to a user. |
| `PUT /api/tasks/{id}/status` | authenticated | Change status. Team Member can change only on tasks assigned to them; Project Manager can change status on tasks in their own projects or assigned to them. |
| `POST /api/tasks/improve-description` | authenticated | Improve a task description using the AI provider. |

**Task status:** `ToDo`, `InProgress`, `Completed`, `Cancelled`.
**Task priority:** `Low`, `Medium`, `High`, `Critical`.

**Task list response shape:** same `PagedResult<T>` envelope as projects.

**TaskResponse:**
```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "projectId": "00000000-0000-0000-0000-000000000000",
  "projectName": "Project name",
  "title": "Task title",
  "description": "Optional description",
  "status": "ToDo",
  "priority": "High",
  "dueDate": "2026-07-23T00:00:00Z",
  "assignedToUserId": "00000000-0000-0000-0000-000000000000",
  "assignedToUserName": "Admin User",
  "createdAt": "2026-07-23T00:00:00Z",
  "updatedAt": "2026-07-23T00:00:00Z"
}
```

### Users — `api/users`
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `GET /api/users/assignees` | `tasks.assign` | Returns users eligible to be assigned to tasks (Team Members only). |
| `GET /api/users` | `users.manage` | Returns all users with their current role assignments. |
| `PUT /api/users/{id}/role` | `users.manage` | Replaces the role assigned to the specified user. Pass `null` or empty string to remove all roles. |

### Dashboard
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `GET /api/dashboard` | authenticated | Basic statistics: total projects, total tasks, tasks by status, tasks by priority, completed vs pending, upcoming due tasks. |

**DashboardResponse:**
```json
{
  "totalProjects": 5,
  "totalTasks": 42,
  "tasksByStatus": { "ToDo": 10, "InProgress": 20, "Completed": 10, "Cancelled": 2 },
  "tasksByPriority": { "Low": 5, "Medium": 20, "High": 12, "Critical": 5 },
  "completedTasks": 10,
  "pendingTasks": 30,
  "upcomingDueTasks": 8
}
```

### AI
| Endpoint | Permission | Description |
|----------|-----------|-------------|
| `GET /api/ai/status` | anonymous | Returns whether the AI description improver is configured (`enabled`). |
| `POST /api/tasks/improve-description` | authenticated | Improve a task description. Requires `Ai:ApiKey` in User Secrets. |

**AI improve request:**
```json
{ "description": "Fix bug." }
```

**AI improve response:**
```json
{
  "improvedDescription": "Investigate, reproduce, and resolve the reported defect while adding regression coverage."
}
```

### Operational
| Endpoint | Description |
|----------|-------------|
| `GET /health` | Health check; returns an `ApiResponse` envelope with the DB check status. |
| `GET /swagger` | Swagger UI (Development only). |
| `GET /swagger/v1/swagger.json` | OpenAPI document (Development only). |

A static export of the OpenAPI document is also available at `docs/swagger-v1.json` for tooling.

### Cross-cutting behavior
- **Consistent responses:** every response uses the `ApiResponse` / `ApiResponse<T>` envelope
  (`success`, `message`, `data`, `errors`).
- **Validation:** FluentValidation via a global `ValidationActionFilter`; invalid requests return
  the consistent error envelope.
- **Global exception handling:** unhandled exceptions are logged and returned as a consistent
  error envelope (no stack traces leaked; details shown only in Development).
- **Authentication/authorization:** JWT bearer with permission-based policies; ownership and
  visibility rules enforced in the service layer.
- **Soft delete:** projects and tasks use soft delete. Deleted rows are excluded automatically
  from all read queries via EF Core `HasQueryFilter`.
- **Logging:** Serilog request logging to console and a rolling daily file (`logs/log-*.txt`).
- **CORS:** named policy `AngularDevClient`, restricted to origins in `Cors:AllowedOrigins`
  (default `http://localhost:4200`).
- **Rate limiting:** fixed-window global limiter, 100 requests/minute per client IP (429 on
  rejection).
- **HTTPS:** HTTPS redirection and HSTS are enabled for production deployments. For production, ensure TLS termination at your reverse proxy or app server.

## Database & Migrations

Applied migrations:

| Migration | Contents |
|-----------|----------|
| `InitialCreate` | Empty baseline. |
| `AddIdentityAndRefreshTokens` | ASP.NET Core Identity tables + `RefreshTokens`. |
| `AddProjects` | `Projects` table. |
| `AddTasks` | `Tasks` table with `Project → Task` cascade delete. |
| `AddSoftDelete` | `Projects.IsDeleted`, `Projects.DeletedAt`, `Projects.DeletedByUserId`, same for `Tasks`. |

## Frontend Architecture

The Angular frontend lives in `client/smart-task-ui/` and follows these conventions:

- **Standalone components** — no NgModules.
- **Angular Material + CDK** — `MatTable`, `MatPaginator`, `MatSort`, `MatDialog`, `MatSnackBar`, `BreakpointObserver`.
- **Signal-based state** — `signal()`, `computed()`, and reactive forms.
- **RxJS cleanup** — `takeUntilDestroyed()` from `@angular/core/rxjs-interop` on all subscriptions.
- **Route structure:**
  - `/login`, `/register` — auth pages
  - `/dashboard` — aggregate stats (recent projects/tasks are loaded client-side)
  - `/projects` — project list with search/sort/pagination
  - `/projects/create` — create project (Admin / Project Manager only)
  - `/projects/:id` — project detail
  - `/projects/:id/edit` — edit project (Admin / Project Manager only)
  - `/tasks` — task list with search/status/priority filters, sort/pagination
  - `/tasks/create` — create task (Admin / Project Manager only)
  - `/tasks/:id` — task detail
  - `/tasks/:id/edit` — edit task (Admin / Project Manager only)
  - `/account` — current user profile
  - `/users` — user management (Admin only)
  - `/403`, `/404` — error pages
- **Guards:** `authGuard` redirects unauthenticated users to `/login`; `roleGuard` restricts
  create/edit routes to Admin and Project Manager; `unsavedChangesGuard` confirms before
  leaving dirty forms.
- **Interceptors:** `authInterceptor` attaches JWT and handles refresh; `errorInterceptor` sanitizes error messages and maps HTTP status codes to user-friendly toasts.
- **AI integration:** the task form includes an AI enhance button. When the AI backend is not
  configured, the button remains visible but disabled, with a tooltip indicating that the
  feature is unavailable. When enabled, the button actively improves task descriptions.
- **Dashboard charts:** the dashboard renders two interactive pie charts. Clicking a slice filters the task list by that status or priority.
- **Login UX:** the email and password fields rely on the browser's native autocomplete.
- **User Management:** Admin-only page for viewing all users and assigning roles. Uses a Material table with per-row role selectors.

## Troubleshooting

### Frontend dependency / build issues

If `npm install` or `npm run build` fails after pulling recent changes (for example, after
Chart.js / ng2-charts was added for the dashboard pie charts), try:

```bash
cd client/smart-task-ui
Remove-Item -Recurse -Force node_modules,dist
npm install
npm run build
```

If you see an error like `connect ETIMEDOUT` for Google Fonts during build, retry the build
once or twice; it is usually a transient network issue.

If `ng build` warns that the bundle exceeds the budget, that is expected after adding
`ng2-charts` + `chart.js`. Production builds still succeed; the warning does not block the
app.

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
npm start                                       # starts the Angular development server. The frontend communicates directly with the API configured in environment.ts.
npm run build                                   # production build
```

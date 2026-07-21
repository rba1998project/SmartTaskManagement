# Phase 6 Implementation Plan: Dashboard

Aggregate statistics endpoint. No new persistence tables or migrations; all data comes from the existing `Projects` and `Tasks` tables via the existing repositories.

* **Milestone:** `GET /api/dashboard` returns all statistics in a single lightweight response.
* **Depends on:** Phases 3, 4, and 5.
* **Scope boundary:** no dashboard-specific data tables, no caching, no background jobs, no AI, and no frontend work.

## Design decisions (resolved)

* **Response shape:** a single DTO containing only aggregate values — no nested lists or paginated data.

  ```csharp
  public sealed class DashboardResponse
  {
      public int TotalProjects { get; init; }
      public int TotalTasks { get; init; }
      public Dictionary<TaskItemStatus, int> TasksByStatus { get; init; } = new();
      public Dictionary<TaskItemPriority, int> TasksByPriority { get; init; } = new();
      public int CompletedTasks { get; init; }
      public int PendingTasks { get; init; }
      public int UpcomingDueTasks { get; init; }
  }
  ```

  * `TotalProjects` / `TotalTasks` — simple counts.
  * `TasksByStatus` — count of tasks grouped by status (`ToDo`, `InProgress`, `Completed`, `Cancelled`). All four keys are always present (zero-filled if absent from the database).
  * `TasksByPriority` — count of tasks grouped by priority (`Low`, `Medium`, `High`, `Critical`). All four keys are always present.
  * `CompletedTasks` — count where `Status == Completed`.
  * `PendingTasks` — count where `Status == ToDo || Status == InProgress` (excludes `Cancelled`).
  * `UpcomingDueTasks` — count where `DueDate >= DateTime.UtcNow.Date && DueDate <= DateTime.UtcNow.Date.AddDays(7)` (due today through the next 7 days; excludes overdue tasks).

* **Authorization:** open to any authenticated user (`[Authorize]` at controller level). Data scope is enforced by role-based visibility rules, consistent with list endpoints.
* **Visibility rules:**
  - **Admin:** sees all projects and tasks.
  - **Project Manager:** sees all projects and tasks for reading (same as `ListAsync` in Phase 5; ownership is enforced only on mutations).
  - **Team Member:** sees only projects/tasks derived from their own assignments (same as `ListAsync` in Phase 5).
* **Database-side aggregation:** all counts are computed in SQL via EF Core (`CountAsync`, `GroupBy` for dictionaries). No entity materialization for aggregation.
* **Dedicated count methods:** do not reuse `QueryAsync` for totals. Add lightweight `CountVisibleAsync` methods to both repositories.

## Implementation approach

### 6A — Dashboard response DTO

Create `SmartTaskManagement.Application/Dashboard/DashboardResponse.cs`.

```csharp
namespace SmartTaskManagement.Application.Dashboard;

using SmartTaskManagement.Domain.Entities;

public sealed class DashboardResponse
{
    public int TotalProjects { get; init; }
    public int TotalTasks { get; init; }
    public Dictionary<TaskItemStatus, int> TasksByStatus { get; init; } = new();
    public Dictionary<TaskItemPriority, int> TasksByPriority { get; init; } = new();
    public int CompletedTasks { get; init; }
    public int PendingTasks { get; init; }
    public int UpcomingDueTasks { get; init; }
}
```

### 6B — Repository aggregation methods

Extend both `IProjectRepository` and `ITaskRepository` with dedicated count methods. Implement them in both repositories.

**`IProjectRepository` additions:**

```csharp
/// <summary>
/// Counts projects visible to the current user scope. When <paramref name="teamMemberUserId"/>
/// is provided, only projects containing a task assigned to that user are counted. Pass
/// <c>null</c> for Admin / Project Manager to count all projects.
/// </summary>
Task<int> CountVisibleAsync(Guid? teamMemberUserId, CancellationToken cancellationToken = default);
```

**`ITaskRepository` additions:**

```csharp
/// <summary>
/// Counts tasks matching the predicate, scoped by visibility parameters.
/// </summary>
Task<int> CountAsync(
    Expression<Func<TaskItem, bool>> predicate,
    Guid? assignedToUserId,
    Guid? projectOwnerUserId,
    CancellationToken cancellationToken = default);

/// <summary>
/// Counts tasks grouped by status, scoped by visibility parameters. All four status values
/// are always present in the returned dictionary (zero-filled if no tasks match).
/// </summary>
Task<Dictionary<TaskItemStatus, int>> CountByStatusAsync(
    Guid? assignedToUserId,
    Guid? projectOwnerUserId,
    CancellationToken cancellationToken = default);

/// <summary>
/// Counts tasks grouped by priority, scoped by visibility parameters. All four priority values
/// are always present in the returned dictionary (zero-filled if no tasks match).
/// </summary>
Task<Dictionary<TaskItemPriority, int>> CountByPriorityAsync(
    Guid? assignedToUserId,
    Guid? projectOwnerUserId,
    CancellationToken cancellationToken = default);
```

**Implementation notes:**

- `ProjectRepository.CountVisibleAsync`: builds `IQueryable` with the same visibility scoping as `QueryAsync` (`teamMemberUserId` semi-join), calls `CountAsync`. No paging, no ordering, no materialization.
- `TaskRepository.CountAsync`: builds `IQueryable` with visibility scoping (`assignedToUserId` filter, `projectOwnerUserId` semi-join), applies the predicate, calls `CountAsync`.
- `TaskRepository.CountByStatusAsync`: builds `IQueryable` with visibility scoping, groups by `Status`, and returns a dictionary with all four enum values present.
- `TaskRepository.CountByPriorityAsync`: builds `IQueryable` with visibility scoping, groups by `Priority`, and returns a dictionary with all four enum values present.
- All methods use `AsNoTracking()`.
- No navigation properties are loaded.

**Dictionary normalization:**

After `GroupBy`, fill missing enum values with `0` before returning:

```csharp
var result = await query
    .GroupBy(t => t.Status)
    .Select(g => new { Status = g.Key, Count = g.Count() })
    .ToListAsync(cancellationToken);

var dict = Enum.GetValues<TaskItemStatus>()
    .ToDictionary(s => s, s => 0);

foreach (var item in result)
    dict[item.Status] = item.Count;

return dict;
```

### 6C — Dashboard service

Create `SmartTaskManagement.Application/Dashboard/DashboardService.cs`.

```csharp
public async Task<Result<DashboardResponse>> GetAsync(CancellationToken cancellationToken = default)
{
    // Visibility scope: same rules as list endpoints.
    var assignedToUserId = _currentUser.IsInRole(RoleNames.TeamMember) ? _currentUser.UserId : null;
    var projectOwnerUserId = _currentUser.IsInRole(RoleNames.ProjectManager) && !_currentUser.IsInRole(RoleNames.Admin)
        ? _currentUser.UserId : null;

    // Total counts — dedicated count methods, no paging.
    var totalProjects = await _projects.CountVisibleAsync(assignedToUserId, cancellationToken);
    var totalTasks = await _tasks.CountAsync(t => true, assignedToUserId, projectOwnerUserId, cancellationToken);

    // Grouped counts — database-side GROUP BY.
    var statusCounts = await _tasks.CountByStatusAsync(assignedToUserId, projectOwnerUserId, cancellationToken);
    var priorityCounts = await _tasks.CountByPriorityAsync(assignedToUserId, projectOwnerUserId, cancellationToken);

    // Derived counts.
    var completedTasks = statusCounts[TaskItemStatus.Completed];
    var pendingTasks = statusCounts[TaskItemStatus.ToDo] + statusCounts[TaskItemStatus.InProgress];
    var upcomingDueTasks = await _tasks.CountAsync(
        t => t.DueDate.HasValue && t.DueDate >= DateTime.UtcNow.Date && t.DueDate <= DateTime.UtcNow.Date.AddDays(7),
        assignedToUserId, projectOwnerUserId, cancellationToken);

    return Result<DashboardResponse>.Success(new DashboardResponse
    {
        TotalProjects = totalProjects,
        TotalTasks = totalTasks,
        TasksByStatus = statusCounts,
        TasksByPriority = priorityCounts,
        CompletedTasks = completedTasks,
        PendingTasks = pendingTasks,
        UpcomingDueTasks = upcomingDueTasks
    });
}
```

Dependencies: `IProjectRepository`, `ITaskRepository`, `ICurrentUserService`.

### 6D — Dashboard controller & API

Create `SmartTaskManagement.API/Controllers/DashboardController.cs`.

```csharp
[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAsync(cancellationToken);
        return Ok(ApiResponse.Ok(result.Value!));
    }
}
```

Register `DashboardService` in DI (e.g., in `Application/DependencyInjection.cs` or via the existing service registration pattern).

## Files to create

| File | Purpose |
|------|---------|
| `SmartTaskManagement.Application/Dashboard/DashboardResponse.cs` | Single DTO with all aggregate fields. |
| `SmartTaskManagement.Application/Dashboard/DashboardService.cs` | Orchestrates counts from repositories, applies visibility scope, maps to response. |
| `SmartTaskManagement.API/Controllers/DashboardController.cs` | Thin `GET /api/dashboard` endpoint. |

## Files to modify

| File | Change |
|------|--------|
| `SmartTaskManagement.Application/Abstractions/IProjectRepository.cs` | Add `CountVisibleAsync`. |
| `SmartTaskManagement.Infrastructure/Persistence/ProjectRepository.cs` | Implement `CountVisibleAsync`. |
| `SmartTaskManagement.Application/Abstractions/ITaskRepository.cs` | Add `CountAsync`, `CountByStatusAsync`, `CountByPriorityAsync`. |
| `SmartTaskManagement.Infrastructure/Persistence/TaskRepository.cs` | Implement the three new methods. |

## Verification

* All authenticated users can call `GET /api/dashboard`.
* Admin sees all projects/tasks.
* Project Manager sees all projects/tasks for reading.
* Team Member sees scoped counts (only their assigned projects/tasks).
* `TasksByStatus` contains all four status values with correct counts (zero-filled if absent).
* `TasksByPriority` contains all four priority values with correct counts (zero-filled if absent).
* `CompletedTasks` counts only `Completed`.
* `PendingTasks` counts `ToDo` + `InProgress`; `Cancelled` is excluded.
* `UpcomingDueTasks` counts tasks with `DueDate >= today && DueDate <= today + 7 days` (excludes overdue).
* All aggregation executes database-side (verify SQL via logs).
* No `QueryAsync` abuse for totals — dedicated count methods are used.
* Build succeeds with 0 warnings and 0 errors.

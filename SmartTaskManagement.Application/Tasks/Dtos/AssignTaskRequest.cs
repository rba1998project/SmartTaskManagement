namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Input for (re)assigning a task. A <c>null</c> <see cref="AssignedToUserId"/> clears the
/// assignment; a non-null value must reference an existing application user.
/// </summary>
public sealed class AssignTaskRequest
{
    public Guid? AssignedToUserId { get; init; }
}

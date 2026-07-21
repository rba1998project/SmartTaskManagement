namespace SmartTaskManagement.Application.Projects.Dtos;

/// <summary>
/// Fields available for sorting project list results.
/// </summary>
public enum ProjectSortFieldDto
{
    /// <summary>Project name.</summary>
    Name = 0,

    /// <summary>Creation timestamp.</summary>
    CreatedAt = 1,

    /// <summary>Last updated timestamp.</summary>
    UpdatedAt = 2
}

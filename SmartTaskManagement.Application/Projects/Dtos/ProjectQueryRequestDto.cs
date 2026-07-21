using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.Application.Projects.Dtos;

/// <summary>
/// Query parameters for the project list endpoint.
/// </summary>
public sealed class ProjectQueryRequestDto
{
    /// <summary>Keyword search across <see cref="ProjectResponseDto.Name"/> and <see cref="ProjectResponseDto.Description"/>.</summary>
    public string? Search { get; init; }

    /// <summary>Field to sort by.</summary>
    public ProjectSortFieldDto SortField { get; init; } = ProjectSortFieldDto.CreatedAt;

    /// <summary>Sort direction.</summary>
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;

    /// <summary>Page number (1-based).</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Page size. Maximum 100.</summary>
    public int PageSize { get; init; } = 20;
}

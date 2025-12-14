namespace CoreLedger.Application.DTOs;

/// <summary>
/// Data transfer object for ToDo entity.
/// </summary>
public record ToDoDto(
    int Id,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

/// <summary>
/// DTO for creating a new ToDo.
/// </summary>
public record CreateToDoDto(string Description);

/// <summary>
/// DTO for updating an existing ToDo.
/// </summary>
public record UpdateToDoDto(string Description, bool IsCompleted);

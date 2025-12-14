using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
/// ToDo domain entity with business rules and invariants.
/// </summary>
public class ToDo : BaseEntity
{
    public string Description { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private ToDo() { }

    /// <summary>
    /// Factory method to create a new ToDo with validation.
    /// </summary>
    public static ToDo Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException("Description cannot be empty");

        if (description.Length > 500)
            throw new DomainValidationException("Description cannot exceed 500 characters");

        return new ToDo
        {
            Description = description,
            IsCompleted = false
        };
    }

    /// <summary>
    /// Updates the description with validation.
    /// </summary>
    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException("Description cannot be empty");

        if (description.Length > 500)
            throw new DomainValidationException("Description cannot exceed 500 characters");

        Description = description;
        SetUpdated();
    }

    /// <summary>
    /// Marks the ToDo as completed.
    /// </summary>
    public void MarkAsCompleted()
    {
        if (IsCompleted)
            throw new DomainValidationException("ToDo is already completed");

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        SetUpdated();
    }

    /// <summary>
    /// Marks the ToDo as incomplete.
    /// </summary>
    public void MarkAsIncomplete()
    {
        if (!IsCompleted)
            throw new DomainValidationException("ToDo is already incomplete");

        IsCompleted = false;
        CompletedAt = null;
        SetUpdated();
    }
}

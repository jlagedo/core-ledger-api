namespace CoreLedger.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-related exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when domain validation fails.
/// </summary>
public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) 
        : base(message, "ERR-DOMAIN-001")
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id) 
        : base($"{entityName} with id {id} not found", "ERR-NOTFOUND-001")
    {
    }
}

namespace core_ledger_api.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; }
    
    public DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object id) 
        : base($"{entityName} with id {id} not found", "ERR-NOTFOUND-001")
    {
    }
}

public class ValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(Dictionary<string, string[]> errors) 
        : base("One or more validation errors occurred", "ERR-VALIDATION-001")
    {
        Errors = errors;
    }
}

public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message = "The record was modified by another user") 
        : base(message, "ERR-CONCURRENCY-001")
    {
    }
}

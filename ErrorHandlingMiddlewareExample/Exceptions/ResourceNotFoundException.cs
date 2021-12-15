namespace ErrorHandlingMiddlewareExample.Exceptions;

public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resourceName, string resourceId)
        : base("Resource not found.")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    public string ResourceName { get; }

    public string ResourceId { get; }
}
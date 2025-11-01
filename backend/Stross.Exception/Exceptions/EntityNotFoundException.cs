namespace Stross.Exception.Exceptions;

public class EntityNotFoundException : StrossException
{
    public EntityNotFoundException(string entityName) : base($"The entity of type {entityName} was not found.")
    {
    }
}
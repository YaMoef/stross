namespace Stross.Exception.Exceptions;

public class EntityAlreadyExistsException : StrossException
{
    public EntityAlreadyExistsException(string entityName) : base($"The entity of type {entityName} already exists.")
    {
    }
}
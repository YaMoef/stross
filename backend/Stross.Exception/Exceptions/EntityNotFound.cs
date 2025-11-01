namespace Stross.Exception.Exceptions;

public class EntityNotFound : StrossException
{
    public EntityNotFound(string entityName) : base($"The entity of type {entityName} was not found.")
    {
    }
}
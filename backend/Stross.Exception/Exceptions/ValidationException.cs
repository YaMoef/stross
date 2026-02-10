namespace Stross.Exception.Exceptions;

public class ValidationException : StrossException
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(Dictionary<string, string> fieldErrors) : base(string.Join(" ", fieldErrors.Select(e => $"Validation error on field: {e.Key}: {e.Value}")))
    {

    }

    public ValidationException(string fieldName, string fieldError) : this(new Dictionary<string, string>
    {
        {
            fieldName, fieldError
        }
    })
    {

    }
}

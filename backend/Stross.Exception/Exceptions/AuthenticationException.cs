namespace Stross.Exception.Exceptions;

public class AuthenticationException : StrossException
{
    public AuthenticationException() : base("Failed to authenticate user.")
    {
    }
}

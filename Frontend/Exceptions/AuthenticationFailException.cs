namespace Frontend.Exceptions;

public class AuthenticationFailException : Exception
{
    public AuthenticationFailException()
        : base("Authentication fail.") {}
    
    public AuthenticationFailException(string message)
        : base(message) {}
}
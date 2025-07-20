public class ContentGuardException : Exception
{
    public ContentGuardException(string message) : base(message) { }
}

public class ValidationException : ContentGuardException
{
    public ValidationException(string message) : base(message) { }
}
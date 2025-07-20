using System;

namespace AI.ContentGuard.Shared.Exceptions;

public class ContentGuardException : Exception
{
    public ContentGuardException(string message) : base(message) { }
}
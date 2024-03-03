namespace GitHubReleaseGen.ConsoleApp;

internal sealed class GitCliException : Exception
{
    public GitCliException(string message, string? gitLogError, GitCliExceptionType exceptionType) : base(message)
    {
        GitLogError = gitLogError;
        ExceptionType = exceptionType;
    }

    public GitCliException(string message, string? gitLogError, GitCliExceptionType exceptionType, Exception innerException) : base(message, innerException)
    {
        GitLogError = gitLogError;
        ExceptionType = exceptionType;
    }

    public GitCliExceptionType ExceptionType { get; }

    public string? GitLogError { get; }
}

internal enum GitCliExceptionType
{
    Unknown,
    GitLogError
}

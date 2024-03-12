using System.Diagnostics;

namespace GitHubReleaseGen.ConsoleApp.Models.Git;

/// <summary>
/// Class for running a 'git' command.
/// </summary>
public class GitProcess : IDisposable
{
    private bool _disposed;
    private readonly ProcessStartInfo _processStartInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitProcess"/> class.
    /// </summary>
    /// <param name="arguments">An array of arguments to pass to the 'git command.</param>
    /// <param name="workingDirectory">The directory to run the 'git' command in.</param>
    public GitProcess(string[] arguments, string? workingDirectory)
    {
        _processStartInfo = new(
            fileName: "git",
            arguments: arguments
        )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
        };
    }

    /// <summary>
    /// Data for the 'git' process.
    /// </summary>
    public Process? ProcessData { get; set; }

    /// <summary>
    /// Runs a 'git' command.
    /// </summary>
    /// <param name="processStartInfo">The <see cref="ProcessStartInfo"/> instance to use for running the 'git' command.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Failed to start git process.</exception>
    /// <exception cref="GitCliException">An error occurred while running the 'git' command.</exception>
    public async Task RunGitProcessAsync()
    {
        ProcessData = Process.Start(_processStartInfo) ?? throw new InvalidOperationException("Failed to start git process.");

        await ProcessData.WaitForExitAsync();

        if (ProcessData.ExitCode != 0)
        {
            string errorOutput = await ProcessData.StandardError.ReadToEndAsync();

            throw new GitCliException("An error occurred while running the 'git' command.", errorOutput, GitCliExceptionType.Unknown);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ProcessData?.Dispose();

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}

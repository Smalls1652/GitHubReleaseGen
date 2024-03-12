using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitHubReleaseGen.ConsoleApp.Models.Git;

/// <summary>
/// Represents information about a commit in a Git repository.
/// </summary>
public sealed partial class CommitInfo
{
    private readonly string _inputRef;
    private readonly string? _repoPath;

    private string? _shaAbbreviated;
    private string? _refName;
    private string? _subject;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommitInfo"/> class.
    /// </summary>
    /// <param name="inputRef">The ref to get the commit information for.</param>
    public CommitInfo(string inputRef) : this(inputRef, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommitInfo"/> class.
    /// </summary>
    /// <param name="inputRef">The ref to get the commit information for.</param>
    /// <param name="repoPath">The path to the local repository.</param>
    public CommitInfo(string inputRef, string? repoPath)
    {
        _inputRef = inputRef;
        _repoPath = repoPath;
    }

    /// <summary>
    /// The abbreviated SHA of the commit.
    /// </summary>
    public string ShaAbbreviated
    {
        get => _shaAbbreviated ?? throw new NullReferenceException("Data has not been retrieved yet.");
    }

    /// <summary>
    /// The name of the ref.
    /// </summary>
    public string RefName
    {
        get => _refName ?? throw new NullReferenceException("Data has not been retrieved yet.");
    }

    /// <summary>
    /// The subject of the commit.
    /// </summary>
    public string Subject
    {
        get => _subject ?? throw new NullReferenceException("Data has not been retrieved yet.");
    }

    /// <summary>
    /// Initializes the <see cref="CommitInfo"/> instance.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">The git log output could not be parsed.</exception>
    public async Task GetCommitInfoAsync()
    {
        ProcessStartInfo processStartInfo = CreateGitProcessStartInfo(
            arguments: [
                "log",
                "--format='%h - %S - %s'",
                "-1",
                _inputRef
            ],
            workingDirectory: _repoPath ?? Environment.CurrentDirectory
        );

        using Process process = await RunGitProcessAsync(processStartInfo);

        string output = await process.StandardOutput.ReadToEndAsync();

        var match = CommitLogRegex().Match(output);

        if (!match.Success)
        {
            throw new InvalidOperationException("The git log output could not be parsed.");
        }

        _shaAbbreviated = match.Groups["sha"].Value;
        _refName = match.Groups["refName"].Value;
        _subject = match.Groups["subject"].Value;
    }

    /// <summary>
    /// Creates a new <see cref="ProcessStartInfo"/> instance for running a 'git' command.
    /// </summary>
    /// <param name="arguments">Arguments to pass to the 'git' command.</param>
    /// <param name="workingDirectory">The directory to run the 'git' command in.</param>
    /// <returns>A new <see cref="ProcessStartInfo"/> instance.</returns>
    private static ProcessStartInfo CreateGitProcessStartInfo(string[] arguments, string? workingDirectory)
    {
        return new(
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
    /// Runs a 'git' command.
    /// </summary>
    /// <param name="processStartInfo">The <see cref="ProcessStartInfo"/> instance to use for running the 'git' command.</param>
    /// <returns>An instance of <see cref="Process"/> representing the running 'git' command.</returns>
    /// <exception cref="InvalidOperationException">Failed to start git process.</exception>
    /// <exception cref="GitCliException">An error occurred while running the 'git' command.</exception>
    private static async Task<Process> RunGitProcessAsync(ProcessStartInfo processStartInfo)
    {
        Process process = Process.Start(processStartInfo) ?? throw new InvalidOperationException("Failed to start git process.");

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            string errorOutput = await process.StandardError.ReadToEndAsync();

            throw new GitCliException("An error occurred while running the 'git' command.", errorOutput, GitCliExceptionType.Unknown);
        }

        return process;
    }

    [GeneratedRegex(
        pattern: "(?'sha'[a-zA-z0-9]{7}) - (?'refName'.+?) - (?'subject'.+)"
    )]
    internal static partial Regex CommitLogRegex();
}

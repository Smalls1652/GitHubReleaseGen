using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitHubReleaseGen.ConsoleApp.Models.Git;

/// <summary>
/// A collection of commits in a Git repository.
/// </summary>
public sealed partial class CommitsCollection
{
    private readonly CommitInfo? _baseCommitRef;
    private readonly CommitInfo? _newCommitRef;
    private readonly string? _repoPath;

    private List<string>? _commits;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommitsCollection"/> class.
    /// </summary>
    /// <param name="baseCommitRef">The base commit ref.</param>
    /// <param name="newCommitRef">The new commit ref.</param>
    public CommitsCollection(CommitInfo baseCommitRef, CommitInfo newCommitRef) : this(baseCommitRef, newCommitRef, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommitsCollection"/> class.
    /// </summary>
    /// <param name="baseCommitRef">The base commit ref.</param>
    /// <param name="newCommitRef">The new commit ref.</param>
    /// <param name="repoPath">The path to the local repository.</param>
    public CommitsCollection(CommitInfo baseCommitRef, CommitInfo newCommitRef, string? repoPath)
    {
        _baseCommitRef = baseCommitRef;
        _newCommitRef = newCommitRef;
        _repoPath = repoPath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommitsCollection"/> class.
    /// </summary>
    /// <param name="commits">An array of commits.</param>
    public CommitsCollection(string[] commits)
    {
        _commits = new(commits);
    }

    /// <summary>
    /// A collection of commit log entries.
    /// </summary>
    public List<string> Commits
    {
        get => _commits ?? throw new NullReferenceException("Data has not been retrieved yet.");
    }

    /// <summary>
    /// Gets the commits between the base and new commit refs.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">A base or new commit ref was not set.</exception>
    public async Task GetCommitsBetweenRefsAsync()
    {
        if (_baseCommitRef is null)
        {
            throw new NullReferenceException("Base commit ref is null.");
        }

        if (_newCommitRef is null)
        {
            throw new NullReferenceException("New commit ref is null.");
        }

        ProcessStartInfo processStartInfo = CreateGitProcessStartInfo(
            arguments: [
                "--no-pager",
                "log",
                $"{_baseCommitRef.RefName}..{_newCommitRef.RefName}",
                "--reverse",
                "--oneline"
            ],
            workingDirectory: _repoPath ?? Environment.CurrentDirectory
        );

        using Process process = await RunGitProcessAsync(processStartInfo);

        string output = await process.StandardOutput.ReadToEndAsync();

        if (_commits is null)
        {
            _commits = [];
        }

        foreach (Match matchItem in CommitRegex().Matches(output))
        {
            if (matchItem.Success && matchItem.Groups["commit"].Success)
            {
                _commits.Add(matchItem.Groups["commit"].Value);
            }
        }
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
        pattern: "(?'commit'[a-zA-z0-9]{7}) .+"
    )]
    internal static partial Regex CommitRegex();
}
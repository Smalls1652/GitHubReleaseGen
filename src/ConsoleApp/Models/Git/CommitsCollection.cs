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

        using GitProcess gitProcess = new(
            arguments: [
                "--no-pager",
                "log",
                $"{_baseCommitRef.RefName}..{_newCommitRef.RefName}",
                "--reverse",
                "--oneline"
            ],
            workingDirectory: _repoPath ?? Environment.CurrentDirectory
        );

        await gitProcess.RunGitProcessAsync();

        if (gitProcess.ProcessData is null)
        {
            throw new InvalidOperationException("The git log process is null.");
        }

        string output = await gitProcess.ProcessData.StandardOutput.ReadToEndAsync();

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

    [GeneratedRegex(
        pattern: "(?'commit'[a-zA-z0-9]{7}) .+"
    )]
    internal static partial Regex CommitRegex();
}

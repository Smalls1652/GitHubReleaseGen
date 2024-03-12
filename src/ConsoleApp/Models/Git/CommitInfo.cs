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
        using GitProcess gitProcess = new(
            arguments: [
                "--no-pager",
                "log",
                "--format='%h - %S - %s'",
                "-1",
                _inputRef
            ],
            workingDirectory: _repoPath ?? Environment.CurrentDirectory
        );

        await gitProcess.RunGitProcessAsync();

        if (gitProcess.ProcessData is null)
        {
            throw new InvalidOperationException("The git log process is null.");
        }

        string output = await gitProcess.ProcessData.StandardOutput.ReadToEndAsync();

        var match = CommitLogRegex().Match(output);

        if (!match.Success)
        {
            throw new InvalidOperationException("The git log output could not be parsed.");
        }

        _shaAbbreviated = match.Groups["sha"].Value;
        _refName = match.Groups["refName"].Value;
        _subject = match.Groups["subject"].Value;
    }

    [GeneratedRegex(
        pattern: "(?'sha'[a-zA-z0-9]{7}) - (?'refName'.+?) - (?'subject'.+)"
    )]
    internal static partial Regex CommitLogRegex();
}

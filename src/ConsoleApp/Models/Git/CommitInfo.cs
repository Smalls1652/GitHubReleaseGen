using System.Text.RegularExpressions;

namespace GitHubReleaseGen.ConsoleApp.Models.Git;

public sealed partial class CommitInfo
{
    public CommitInfo(string gitLogOutput)
    {
        var match = CommitLogRegex().Match(gitLogOutput);
        if (!match.Success)
        {
            throw new InvalidOperationException("The git log output could not be parsed.");
        }

        ShaAbbreviated = match.Groups["sha"].Value;
        RefName = match.Groups["refName"].Value;
        Subject = match.Groups["subject"].Value;
    }

    public string ShaAbbreviated { get; set; } = null!;

    public string RefName { get; set; } = null!;

    public string Subject { get; set; } = null!;

    [GeneratedRegex(
        pattern: "(?'sha'[a-zA-z0-9]{7}) - (?'refName'.+?) - (?'subject'.+)"
    )]
    internal static partial Regex CommitLogRegex();
}

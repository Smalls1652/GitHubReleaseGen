namespace GitHubReleaseGen.ConsoleApp.Models.GitHub;

/// <summary>
/// Represents a GitHub pull request merge commit.
/// </summary>
public class GitHubPullRequestMergeCommit
{
    /// <summary>
    /// The SHA of the commit.
    /// </summary>
    [JsonPropertyName("oid")]
    public string? Oid { get; set; }

    /// <summary>
    /// The shortned SHA of the commit.
    /// </summary>
    [JsonIgnore]
    public string? ShortOid => Oid?[..7];
}
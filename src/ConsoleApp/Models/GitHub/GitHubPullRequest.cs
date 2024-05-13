namespace GitHubReleaseGen.ConsoleApp.Models.GitHub;

/// <summary>
/// Represents a GitHub pull request.
/// </summary>
public class GitHubPullRequest
{
    /// <summary>
    /// The number of the pull request.
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; set; }

    /// <summary>
    /// The title of the pull request.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// The URL of the pull request.
    /// </summary>
    [JsonPropertyName("html_url")]
    public Uri HtmlUrl { get; set; } = null!;

    /// <summary>
    /// The state of the pull request.
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = null!;

    /// <summary>
    /// The user who created the pull request.
    /// </summary>
    [JsonPropertyName("user")]
    public GitHubUser User { get; set; } = null!;

    /// <summary>
    /// The merge commit of the pull request.
    /// </summary>
    [JsonPropertyName("mergeCommit")]
    public GitHubPullRequestMergeCommit MergeCommit { get; set; } = null!;

    /// <summary>
    /// The author of the pull request.
    /// </summary>
    [JsonPropertyName("author")]
    public GitHubUser Author { get; set; } = null!;

    /// <summary>
    /// Labels associated with the pull request.
    /// </summary>
    [JsonPropertyName("labels")]
    public GitHubPullRequestLabel[] Labels { get; set; } = [];
}

namespace GitHubReleaseGen.ConsoleApp.Models.GitHub;

/// <summary>
/// Represents a GitHub repository.
/// </summary>
public class GitHubRepo
{
    /// <summary>
    /// The URL of the repository.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
}

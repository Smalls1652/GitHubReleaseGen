namespace GitHubReleaseGen.ConsoleApp.Models.GitHub;

/// <summary>
/// Represents a GitHub pull request label.
/// </summary>
public class GitHubPullRequestLabel
{
    /// <summary>
    /// The ID of the label.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// The name of the label.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The description of the label.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The color of the label.
    /// </summary>
    [JsonPropertyName("color")]
    public string Color { get; set; } = null!;
}

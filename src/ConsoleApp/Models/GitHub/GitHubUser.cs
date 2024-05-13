namespace GitHubReleaseGen.ConsoleApp.Models.GitHub;

/// <summary>
/// Represents a GitHub user.
/// </summary>
public class GitHubUser
{
    /// <summary>
    /// The user's username.
    /// </summary>
    [JsonPropertyName("login")]
    public string Login { get; set; } = null!;

    /// <summary>
    /// The URL of the user's profile.
    /// </summary>
    [JsonPropertyName("html_url")]
    public Uri HtmlUrl { get; set; } = null!;

    /// <summary>
    /// The type of user.
    /// </summary>
    [JsonPropertyName("type")]
    public string UserType { get; set; } = null!;

    /// <summary>
    /// Indicates whether the user is a bot.
    /// </summary>
    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }
}

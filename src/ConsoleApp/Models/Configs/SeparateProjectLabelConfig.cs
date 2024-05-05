namespace GitHubReleaseGen.ConsoleApp.Models.Configs;

/// <summary>
/// Configuration options for the separate project label.
/// </summary>
public sealed class SeparateProjectLabelConfig
{
    /// <summary>
    /// Whether the separate project label is enabled.
    /// </summary>
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = false;

    /// <summary>
    /// The project labels.
    /// </summary>
    [JsonPropertyName("projectLabels")]
    public ProjectLabelItem[] ProjectLabels { get; set; } = [];
}
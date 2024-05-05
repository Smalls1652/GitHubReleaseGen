namespace GitHubReleaseGen.ConsoleApp.Models.Configs;

/// <summary>
/// Defines a project label item.
/// </summary>
public sealed class ProjectLabelItem
{
    /// <summary>
    /// The display name of the label.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The label value.
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = null!;
}

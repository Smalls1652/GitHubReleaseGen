namespace GitHubReleaseGen.ConsoleApp.Models.Configs;

/// <summary>
/// The root of the configuration file.
/// </summary>
public sealed class RootConfig
{
    /// <summary>
    /// Configuration options for the labels.
    /// </summary>
    public LabelsConfig Labels { get; set; } = new();
}
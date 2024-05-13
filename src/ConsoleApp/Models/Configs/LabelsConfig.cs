namespace GitHubReleaseGen.ConsoleApp.Models.Configs;

/// <summary>
/// Holds data for labels configuration.
/// </summary>
public sealed class LabelsConfig
{
    /*

    Disabling these for now, as they are not used in the current implementation.

    /// <summary>
    /// Labels for features.
    /// </summary>
    [JsonPropertyName("feature")]
    public string[] FeatureLabels { get; set; } = [
        "new feature",
    ];

    /// <summary>
    /// Labels for enhancements.
    /// </summary>
    [JsonPropertyName("enhancement")]
    public string[] EnhancementLabels { get; set; } = [
        "enhancement",
    ];

    */

    /// <summary>
    /// Labels for bugs.
    /// </summary>
    [JsonPropertyName("bug")]
    public string[] BugLabels { get; set; } = [
        "bug",
        "bug fix",
        "bugfix",
    ];

    /// <summary>
    /// Labels for maintenance.
    /// </summary>
    [JsonPropertyName("maintenance")]
    public string[] MaintenanceLabels { get; set; } = [
        "maintenance"
    ];
}

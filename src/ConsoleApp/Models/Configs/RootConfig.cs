using GitHubReleaseGen.ConsoleApp.Models.Git;

namespace GitHubReleaseGen.ConsoleApp.Models.Configs;

/// <summary>
/// The root of the configuration file.
/// </summary>
public sealed class RootConfig
{
    /// <summary>
    /// Configuration options for the labels.
    /// </summary>
    [JsonPropertyName("labels")]
    public LabelsConfig Labels { get; set; } = new();

    /// <summary>
    /// Configuration options for the separate project label.
    /// </summary>
    [JsonPropertyName("separateProjectLabel")]
    public SeparateProjectLabelConfig SeparateProjectLabel { get; set; } = new();

    /// <summary>
    /// Gets the configuration file.
    /// </summary>
    /// <returns>Data from the configuration file.</returns>
    /// <exception cref="FileNotFoundException">The configuration file was not found.</exception>
    /// <exception cref="InvalidOperationException">Failed to deserialize the configuration file.</exception>
    public static async Task<RootConfig> GetConfigAsync() => await GetConfigAsync(Environment.CurrentDirectory);

    /// <summary>
    /// Gets the configuration file.
    /// </summary>
    /// <param name="workingDirectory">The working directory.</param>
    /// <returns>Data from the configuration file.</returns>
    /// <exception cref="FileNotFoundException">The configuration file was not found.</exception>
    /// <exception cref="InvalidOperationException">Failed to deserialize the configuration file.</exception>
    public static async Task<RootConfig> GetConfigAsync(string workingDirectory)
    {
        RootGitDirectory rootGitDirectory = new(workingDirectory);

        string configPath = Path.Combine(rootGitDirectory.Path, ".gh-releasegen.json");

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException("Configuration file not found.", configPath);
        }

        string json = await File.ReadAllTextAsync(configPath);

        RootConfig config = JsonSerializer.Deserialize(
            json: json,
            jsonTypeInfo: ConfigJsonContext.Default.RootConfig
        ) ?? throw new InvalidOperationException("Failed to deserialize the configuration file.");

        return config;
    }
}
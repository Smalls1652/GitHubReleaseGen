using GitHubReleaseGen.ConsoleApp.Models.Configs;

namespace GitHubReleaseGen.ConsoleApp;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never
)]
[JsonSerializable(typeof(RootConfig))]
[JsonSerializable(typeof(LabelsConfig))]
internal partial class ConfigJsonContext : JsonSerializerContext
{}
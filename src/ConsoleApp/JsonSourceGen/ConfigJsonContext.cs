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
[JsonSerializable(typeof(SeparateProjectLabelConfig))]
[JsonSerializable(typeof(ProjectLabelItem))]
internal partial class ConfigJsonContext : JsonSerializerContext
{}
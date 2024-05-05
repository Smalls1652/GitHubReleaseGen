using System.CommandLine;
using System.CommandLine.Invocation;

using GitHubReleaseGen.ConsoleApp.Models.Configs;
using GitHubReleaseGen.ConsoleApp.Utilities;

namespace GitHubReleaseGen.ConsoleApp.Commands.Configs;

/// <summary>
/// Action for running the 'config init' command.
/// </summary>
public sealed class ConfigInitCommandAction : AsynchronousCliAction
{
    /// <summary>
    /// Invokes the action.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        ConfigInitCommandOptions options;
        try
        {
            options = new(parseResult);
        }
        catch (Exception ex)
        {
            ConsoleUtils.WriteError(ex.Message);
            return 1;
        }

        string configPath = Path.Combine(options.LocalRepoPath, ".gh-releasegen.json");

        if (File.Exists(configPath) && !options.Force)
        {
            ConsoleUtils.WriteError("The configuration file already exists. Use the '--force' option to overwrite the file.");
            return 1;
        }

        if (File.Exists(configPath) && options.Force)
        {
            File.Delete(configPath);
        }

        RootConfig rootConfig = new();

        string rootConfigJson = JsonSerializer.Serialize(
            value: rootConfig,
            jsonTypeInfo: ConfigJsonContext.Default.RootConfig
        );

        await File.WriteAllTextAsync(configPath, rootConfigJson);

        ConsoleUtils.WriteSuccess($"The configuration file has been initialized at '{configPath}'.");

        return 0;
    }
}
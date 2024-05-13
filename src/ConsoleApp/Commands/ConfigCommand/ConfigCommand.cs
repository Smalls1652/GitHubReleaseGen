using System.CommandLine;

namespace GitHubReleaseGen.ConsoleApp.Commands.Configs;

/// <summary>
/// Commands for managing the configuration file.
/// </summary>
public sealed class ConfigCommand : CliCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigCommand"/> class.
    /// </summary>
    public ConfigCommand() : base("config")
    {
        Description = "Commands for managing the configuration file.";

        Add(new ConfigInitCommand());
    }
}

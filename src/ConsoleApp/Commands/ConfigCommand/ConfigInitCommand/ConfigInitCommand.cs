using System.CommandLine;

namespace GitHubReleaseGen.ConsoleApp.Commands.Configs;

/// <summary>
/// Command to initialize the configuration file.
/// </summary>
public sealed class ConfigInitCommand : CliCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigInitCommand"/> class.
    /// </summary>
    public ConfigInitCommand() : base("init")
    {
        Description = "Initializes the configuration file.";

        Options.Add(
            new CliOption<string>("--local-repo-path")
            {
                Description = "The local path to the repository."
            }
        );

        Options.Add(
            new CliOption<bool>("--force")
            {
                Description = "Force the initialization of the configuration file."
            }
        );

        Action = new ConfigInitCommandAction();
    }
}
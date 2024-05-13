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

        Options
            .AddLocalRepoPathOption()
            .AddForceOption();

        Action = new ConfigInitCommandAction();
    }
}

file static class ConfigInitCommandExtensions
{
    /// <summary>
    /// Add the CLI option <c>--local-repo-path</c> to the <see cref="IList{CliOption}"/>.
    /// </summary>
    /// <param name="options">The <see cref="IList{CliOption}"/> to add the option to.</param>
    /// <returns>The <see cref="IList{CliOption}"/> for chaining.</returns>
    public static IList<CliOption> AddLocalRepoPathOption(this IList<CliOption> options)
    {
        options.Add(
            new CliOption<string>("--local-repo-path")
            {
                Description = "The local path to the repository."
            }
        );

        return options;
    }

    /// <summary>
    /// Add the CLI option <c>--force</c> to the <see cref="IList{CliOption}"/>.
    /// </summary>
    /// <param name="options">The <see cref="IList{CliOption}"/> to add the option to.</param>
    /// <returns>The <see cref="IList{CliOption}"/> for chaining.</returns>
    public static IList<CliOption> AddForceOption(this IList<CliOption> options)
    {
        options.Add(
            new CliOption<bool>("--force")
            {
                Description = "Force the initialization of the configuration file."
            }
        );

        return options;
    }
}

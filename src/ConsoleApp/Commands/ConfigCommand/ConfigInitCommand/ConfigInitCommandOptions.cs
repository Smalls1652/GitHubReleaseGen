using System.CommandLine;

using GitHubReleaseGen.ConsoleApp.Models.Git;

namespace GitHubReleaseGen.ConsoleApp.Commands.Configs;

/// <summary>
/// Options for the 'config init' command.
/// </summary>
public sealed class ConfigInitCommandOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigInitCommandOptions"/> class.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    public ConfigInitCommandOptions(ParseResult parseResult)
    {
        LocalRepoPath = ParseLocalRepoPathArgument(parseResult);
        Force = ParseForceArgument(parseResult);
    }

    /// <summary>
    /// The path to the local repository.
    /// </summary>
    public string LocalRepoPath { get; set; }

    /// <summary>
    /// Force the initialization of the configuration file.
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// Parse the '--local-repo-path' argument from the command line.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    /// <returns>The local repository path.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the local repository path does not exist.</exception>
    private static string ParseLocalRepoPathArgument(ParseResult parseResult)
    {
        string? localRepoPath = parseResult.GetValue<string>("--local-repo-path");

        string repoPath;

        if (localRepoPath is not null)
        {
            string localRepoPathFull = Path.GetFullPath(localRepoPath);

            if (!Directory.Exists(localRepoPathFull))
            {
                throw new DirectoryNotFoundException($"The directory '{localRepoPathFull}' does not exist.");
            }

            RootGitDirectory _ = new(localRepoPathFull);

            repoPath = _.Path;
        }
        else
        {
            repoPath = new RootGitDirectory().Path;
        }

        return repoPath;
    }

    /// <summary>
    /// Parse the '--force' argument from the command line.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    /// <returns>Whether to force the initialization of the configuration file.</returns>
    private static bool ParseForceArgument(ParseResult parseResult)
    {
        return parseResult.GetValue<bool>("--force");
    }
}

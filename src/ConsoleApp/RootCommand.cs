using System.CommandLine;
using GitHubReleaseGen.ConsoleApp.Commands;
using GitHubReleaseGen.ConsoleApp.Commands.Configs;

namespace GitHubReleaseGen.ConsoleApp;

/// <summary>
/// Root command for the CLI tool.
/// </summary>
public class RootCommand : CliRootCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootCommand"/> class.
    /// </summary>
    public RootCommand() : base("GitHub Release Text Maker")
    {
        Description = "Easily create release notes for GitHub releases.";

        Add(new CreateTextCommand());
        Add(new ConfigCommand());
    }
}
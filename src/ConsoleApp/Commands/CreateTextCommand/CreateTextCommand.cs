using System.CommandLine;

namespace GitHubReleaseGen.ConsoleApp.Commands;

/// <summary>
/// Command to create the text for a GitHub release.
/// </summary>
public class CreateTextCommand : CliCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTextCommand"/> class.
    /// </summary>
    public CreateTextCommand() : base("create-text")
    {
        Description = "Create text for a GitHub release between a previous tag and now.";

        Options.Add(
            new CliOption<string>("--base-tag")
            {
                Description = "The base tag to compare against.",
                Required = true
            }
        );

        Options.Add(
            new CliOption<string>("--new-tag")
            {
                Description = "The new tag to compare against.",
                Required = true
            }
        );

        Options.Add(
            new CliOption<string>("--repo-owner")
            {
                Description = "The owner of the repository."
            }
        );

        Options.Add(
            new CliOption<string>("--repo")
            {
                Description = "The repository name."
            }
        );

        Options.Add(
            new CliOption<string>("--local-repo-path")
            {
                Description = "The local path to the repository."
            }
        );

        Options.Add(
            new CliOption<bool>("--exclude-overview-section")
            {
                Description = "Exclude the overview section from the text."
            }
        );

        Action = new CreateTextCommandAction();
    }
}
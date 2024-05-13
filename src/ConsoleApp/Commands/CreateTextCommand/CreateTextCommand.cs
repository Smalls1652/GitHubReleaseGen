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
        Description = "Create text for a GitHub release between two commits.";

        Options.Add(
            new CliOption<string>("--base-ref")
            {
                Description = "The base ref to compare against.",
                Required = true
            }
        );

        Options.Add(
            new CliOption<string>("--target-ref")
            {
                Description = "The target ref to compare against.",
                Required = true,
                DefaultValueFactory = (defaultValue) => "HEAD"
            }
        );

        Options.Add(
            new CliOption<string>("--repo-owner")
            {
                Description = "The owner of the repository."
            }
        );

        Options.Add(
            new CliOption<string>("--repo-name")
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

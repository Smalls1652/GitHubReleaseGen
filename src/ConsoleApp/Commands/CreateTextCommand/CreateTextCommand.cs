using System.CommandLine;
using System.CommandLine.Completions;

using GitHubReleaseGen.ConsoleApp.Models.Git;

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

        GitTags gitTags = new();

        Options
            .AddBaseRefOption(gitTags)
            .AddTargetRefOption(gitTags)
            .AddRepoOwnerOption()
            .AddRepoNameOption()
            .AddLocalRepoPathOption()
            .AddExcludeOverviewSectionOption();

        Action = new CreateTextCommandAction();
    }
}

file static class CreateTextCommandExtensions
{
    /// <summary>
    /// Add the CLI option <c>--base-ref</c> to the <see cref="IList{CliOption}"/>.
    /// </summary>
    /// <param name="options">The <see cref="IList{CliOption}"/> to add the option to.</param>
    /// <param name="gitTags">The current repo's tags.</param>
    /// <returns>The <see cref="IList{CliOption}"/> for chaining.</returns>
    public static IList<CliOption> AddBaseRefOption(this IList<CliOption> options, GitTags gitTags)
    {
        CliOption<string> baseRefOption = new CliOption<string>("--base-ref")
        {
            Description = "The base ref to compare against.",
            Required = true
        };

        string? latestTag = gitTags.GetLatestTag();

        if (latestTag is not null)
        {
            baseRefOption.DefaultValueFactory = (defaultValue) => latestTag;
        }

        baseRefOption.CompletionSources.Add(
            (CompletionContext completionContext) =>
            {
                if (gitTags.Tags.Length == 0)
                {
                    return [];
                }

                string? inputValue = completionContext.ParseResult.GetValue(baseRefOption);

                return inputValue is null
                    ? gitTags.GetLatestTags()
                    : gitTags.FindTags(inputValue);
            }
        );

        options.Add(baseRefOption);

        return options;
    }

    /// <summary>
    /// Add the CLI option <c>--target-ref</c> to the <see cref="IList{CliOption}"/>.
    /// </summary>
    /// <param name="options">The <see cref="IList{CliOption}"/> to add the option to.</param>
    /// <param name="gitTags">The current repo's tags.</param>
    /// <returns>The <see cref="IList{CliOption}"/> for chaining.</returns>
    public static IList<CliOption> AddTargetRefOption(this IList<CliOption> options, GitTags gitTags)
    {
        CliOption<string> targetRefOption = new CliOption<string>("--target-ref")
        {
            Description = "The target ref to compare against.",
            Required = true,
            DefaultValueFactory = (defaultValue) => "HEAD"
        };

        targetRefOption.CompletionSources.Add(
            (CompletionContext completionContext) =>
            {
                if (gitTags.Tags.Length == 0)
                {
                    return [];
                }

                string? inputValue = completionContext.ParseResult.GetValue(targetRefOption);

                return inputValue is null
                    ? gitTags.GetLatestTags()
                    : gitTags.FindTags(inputValue);
            }
        );

        options.Add(targetRefOption);

        return options;
    }

    /// <summary>
    /// Add the CLI option <c>--repo-owner</c> to the <see cref="IList{CliOption}"/>.
    /// </summary>
    /// <param name="options">The <see cref="IList{CliOption}"/> to add the option to.</param>
    /// <returns>The <see cref="IList{CliOption}"/> for chaining.</returns>
    public static IList<CliOption> AddRepoOwnerOption(this IList<CliOption> options)
    {
        options.Add(
            new CliOption<string>("--repo-owner")
            {
                Description = "The owner of the repository."
            }
        );

        return options;
    }

    /// <summary>
    /// Add the CLI option <c>--repo-name</c> to the <see cref="IList{CliOption}"/>.
    /// </summary>
    /// <param name="options">The <see cref="IList{CliOption}"/> to add the option to.</param>
    /// <returns>The <see cref="IList{CliOption}"/> for chaining.</returns>
    public static IList<CliOption> AddRepoNameOption(this IList<CliOption> options)
    {
        options.Add(
            new CliOption<string>("--repo-name")
            {
                Description = "The repository name."
            }
        );

        return options;
    }

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
    /// Add the CLI option <c>--exclude-overview-section</c> to the <see cref="IList{CliOption}"/>.
    /// </summary>
    /// <param name="options">The <see cref="IList{CliOption}"/> to add the option to.</param>
    /// <returns>The <see cref="IList{CliOption}"/> for chaining.</returns>
    public static IList<CliOption> AddExcludeOverviewSectionOption(this IList<CliOption> options)
    {
        options.Add(
            new CliOption<bool>("--exclude-overview-section")
            {
                Description = "Exclude the overview section from the text."
            }
        );

        return options;
    }
}

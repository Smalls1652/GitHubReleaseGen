using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using System.Text.RegularExpressions;
using GitHubReleaseGen.ConsoleApp.Models.Commands;
using GitHubReleaseGen.ConsoleApp.Models.Git;
using GitHubReleaseGen.ConsoleApp.Models.GitHub;
using GitHubReleaseGen.ConsoleApp.Utilities;

namespace GitHubReleaseGen.ConsoleApp.Commands;

/// <summary>
/// Action for the <see cref="CreateTextCommand"/> class.
/// </summary>
public partial class CreateTextCommandAction : AsynchronousCliAction
{
    /// <summary>
    /// Invokes the action.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The exit code.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        // Parse the command line arguments.
        CreateTextCommandOptions options;
        try
        {
            options = new(parseResult);
        }
        catch (Exception ex)
        {
            ConsoleUtils.WriteError(ex.Message);
            return 1;
        }

        CommitInfo baseCommitRef = new(options.BaseRef, options.LocalRepoPath);
        CommitInfo newCommitRef = new(options.TargetRef, options.LocalRepoPath);
        try
        {
            await baseCommitRef.GetCommitInfoAsync();
            await newCommitRef.GetCommitInfoAsync();
        }
        catch (Exception ex)
        {
            ConsoleUtils.WriteError(ex.Message);
            return 1;
        }

        // Get the merged pull requests for the repository.
        GitHubPullRequest[] pullRequests;
        try
        {
            pullRequests = await GhCliUtils.GetMergedPullRequests(
                repoOwner: options.RepoOwner,
                repo: options.RepoName,
                repoPath: options.LocalRepoPath
            );
        }
        catch (Exception ex)
        {
            ConsoleUtils.WriteError(ex.Message);
            return 1;
        }

        // Get commits between the two tags.
        CommitsCollection commitsSinceTag = new(baseCommitRef, newCommitRef, options.LocalRepoPath);
        try
        {
            await commitsSinceTag.GetCommitsBetweenRefsAsync();
        }
        catch (Exception ex)
        {
            ConsoleUtils.WriteError(ex.Message);
            return 1;
        }

        // Get the URL of the repository.
        string repoUrl;
        try
        {
            repoUrl = await GhCliUtils.GetRepoUrlAsync(
                repoOwner: options.RepoOwner,
                repo: options.RepoName,
                repoPath: options.LocalRepoPath
            );
        }
        catch (Exception ex)
        {
            ConsoleUtils.WriteError(ex.Message);
            return 1;
        }

        string[] bugLabels = [
            "bug",
            "bug fix",
            "bugfix",
        ];

        // Filter the pull requests to only include those
        // that have a merge commit associated with a pull request.
        GitHubPullRequest[] pullRequestsSinceTag = Array.FindAll(
            array: pullRequests,
            match: pr => pr.MergeCommit.ShortOid is not null && commitsSinceTag.Commits.Contains(pr.MergeCommit.ShortOid)
        );

        Array.Sort(
            array: pullRequestsSinceTag,
            comparison: (pr1, pr2) => pr1.Number.CompareTo(pr2.Number)
        );

        // Filter the pull requests only include those
        // that are not from a bot and do not have a bug label.
        GitHubPullRequest[] pullRequestsByUser = Array.FindAll(
            array: pullRequestsSinceTag,
            match: pr => pr.Author.IsBot == false && !pr.Labels.Any(label => bugLabels.Contains(label.Name))
        );

        // Filter the pull requests only include those
        // that are not from a bot and have a bug label.
        GitHubPullRequest[] bugFixPrs = Array.FindAll(
            array: pullRequestsSinceTag,
            match: pr => pr.Author.IsBot == false && pr.Labels.Any(label => bugLabels.Contains(label.Name) == true)
        );

        // Filter the pull requests only include those
        // that are from the dependabot bot.
        GitHubPullRequest[] dependencyUpdatePrs = Array.FindAll(
            array: pullRequestsSinceTag,
            match: pr => pr.Author.Login == "app/dependabot"
        );

        // Start building the release text.
        StringBuilder releaseText = new();

        // Add the overview section to the release text,
        // if '--exclude-overview-section' is not provided.
        if (!options.ExcludeOverviewSection)
        {
            releaseText.AppendLine("## Overview");
            releaseText.AppendLine("\nAdd an overview of the changes here...\n");
        }

        // Add the what's changed section to the release text,
        // if there are any.
        if (pullRequestsByUser.Length != 0)
        {
            releaseText.AppendLine("### ✍️ What's Changed\n");

            foreach (var prItem in pullRequestsByUser)
            {
                releaseText.AppendLine($"* {prItem.Title} by @{prItem.Author.Login} in #{prItem.Number}");
            }
        }

        // Add the bug fixes section to the release text,
        // if there are any.
        if (bugFixPrs.Length != 0)
        {
            releaseText.AppendLine("\n### 🪳 Bug Fixes\n");

            foreach (var prItem in bugFixPrs)
            {
                releaseText.AppendLine($"* {prItem.Title} by @{prItem.Author.Login} in #{prItem.Number}");
            }
        }

        // Add the dependency updates section to the release text,
        // if there are any.
        if (dependencyUpdatePrs.Length != 0)
        {
            releaseText.AppendLine("\n### ⛓️ Dependency updates\n");

            foreach (var prItem in dependencyUpdatePrs)
            {
                string dependencyUpdateTitle = PrettifyDependencyUpdateText(prItem.Title);
                releaseText.AppendLine($"* {dependencyUpdateTitle} by @{prItem.Author.Login.Replace("app/", "")} in #{prItem.Number}");
            }
        }

        // Add the full changelog section to the release text.
        releaseText.AppendLine($"\n**Full Changelog**: [`{baseCommitRef.RefName}...{newCommitRef.RefName}`]({repoUrl}/compare/{baseCommitRef.RefName}...{newCommitRef.RefName})");

        // Write the release text to the console.
        ConsoleUtils.WriteOutput(releaseText.ToString());

        return 0;
    }

    /// <summary>
    /// Prettifies the dependency update text.
    /// </summary>
    /// <param name="text">The text to prettify.</param>
    /// <returns>The prettified text.</returns>
    private static string PrettifyDependencyUpdateText(string text)
    {
        if (!DependencyUpdateRegex().IsMatch(text))
        {
            return text;
        }

        return DependencyUpdateRegex().Replace(
            input: text,
            replacement: "Bump **$1** from `$2` to `$3` in `$4`"
        );
    }

    [GeneratedRegex(
        pattern: "Bump (?'dependencyName'.+?) from (?'previousVersion'.+?) to (?'newVersion'.+?)(?>$| in (?'projectPath'.+))"
    )]
    internal static partial Regex DependencyUpdateRegex();
}
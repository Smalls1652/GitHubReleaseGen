using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

using GitHubReleaseGen.ConsoleApp.Extensions.ReleaseText;
using GitHubReleaseGen.ConsoleApp.Models.Commands;
using GitHubReleaseGen.ConsoleApp.Models.Configs;
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

        RootConfig config;
        try
        {
            config = await RootConfig.GetConfigAsync(options.LocalRepoPath);
        }
        catch (FileNotFoundException)
        {
            config = new();
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

        // Start building the release text.
        StringBuilder releaseText = new();

        // Add the overview section to the release text,
        // if '--exclude-overview-section' is not provided.
        if (!options.ExcludeOverviewSection)
        {
            releaseText
                .AppendLine("## Overview")
                .AppendLine()
                .AppendLine("Add an overview of the changes here...")
                .AppendLine();
        }

        if (config.SeparateProjectLabel.Enable)
        {
            foreach (ProjectLabelItem projectLabel in config.SeparateProjectLabel.ProjectLabels)
            {
                GitHubPullRequest[] featureAndEnhancementPrs = GetFeatureAndEnhancementPullRequests(pullRequestsSinceTag, config, projectLabel.Label);
                GitHubPullRequest[] bugFixPrsForProject = GetBugFixPullRequests(pullRequestsSinceTag, config, projectLabel.Label);

                if (featureAndEnhancementPrs.Length != 0 && bugFixPrsForProject.Length != 0)
                {
                    releaseText
                        .AppendLine($"### {projectLabel.Name}")
                        .AppendLine()
                        .AddWhatsChangedSection(featureAndEnhancementPrs, true)
                        .AddBugFixesSection(bugFixPrsForProject, true);
                }
            }

            GitHubPullRequest[] otherFeatureAndEnhancementPrs = GetOtherFeatureAndEnhancementPullRequests(pullRequestsSinceTag, config);
            GitHubPullRequest[] otherBugFixPrs = GetOtherBugFixPullRequests(pullRequestsSinceTag, config);

            if (otherFeatureAndEnhancementPrs.Length != 0 || otherBugFixPrs.Length != 0)
            {
                releaseText
                    .AppendLine("### Other")
                    .AppendLine()
                    .AddWhatsChangedSection(otherFeatureAndEnhancementPrs, true)
                    .AddBugFixesSection(otherBugFixPrs, true);
            }
        }
        else
        {
            GitHubPullRequest[] featureAndEnhancementPrs = GetFeatureAndEnhancementPullRequests(pullRequestsSinceTag, config, null);
            GitHubPullRequest[] bugFixPrs = GetBugFixPullRequests(pullRequestsSinceTag, config, null);

            releaseText
                .AddWhatsChangedSection(featureAndEnhancementPrs, false)
                .AddBugFixesSection(bugFixPrs, false);
        }

        GitHubPullRequest[] maintenancePrs = GetMaintenancePullRequests(pullRequestsSinceTag, config);
        releaseText
            .AddMaintenanceSection(maintenancePrs);

        GitHubPullRequest[] dependencyUpdatePrs = GetDependencyUpdatePullRequests(pullRequestsSinceTag, config);
        releaseText
            .AddDependencyUpdatesSection(dependencyUpdatePrs);


        // Add the full changelog section to the release text.
        releaseText.AppendLine($"**Full Changelog**: [`{baseCommitRef.RefName}...{newCommitRef.RefName}`]({repoUrl}/compare/{baseCommitRef.RefName}...{newCommitRef.RefName})");

        // Write the release text to the console.
        ConsoleUtils.WriteOutput(releaseText.ToString());

        return 0;
    }

    /// <summary>
    /// Gets the feature and enhancement pull requests.
    /// </summary>
    /// <param name="pullRequests">The pull requests.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="projectLabel">The project label.</param>
    /// <returns>The feature and enhancement pull requests.</returns>
    private static GitHubPullRequest[] GetFeatureAndEnhancementPullRequests(GitHubPullRequest[] pullRequests, RootConfig config, string? projectLabel)
    {
        return projectLabel is null
            ? Array.FindAll(
                array: pullRequests,
                match: pr =>
                    pr.Author.IsBot == false
                    && !pr.Labels.Any(label => config.Labels.BugLabels.Contains(label.Name) || config.Labels.MaintenanceLabels.Contains(label.Name))
            )
            : Array.FindAll(
                array: pullRequests,
                match: pr =>
                    pr.Author.IsBot == false
                    && !pr.Labels.Any(label => config.Labels.BugLabels.Contains(label.Name) || config.Labels.MaintenanceLabels.Contains(label.Name))
                    && pr.Labels.Any(label => label.Name == projectLabel)
            );
    }

    /// <summary>
    /// Gets the other feature and enhancement pull requests.
    /// </summary>
    /// <param name="pullRequests">The pull requests.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>All other feature and enhancement pull requests.</returns>
    private static GitHubPullRequest[] GetOtherFeatureAndEnhancementPullRequests(GitHubPullRequest[] pullRequests, RootConfig config)
    {
        return Array.FindAll(
            array: pullRequests,
            match: pr =>
                pr.Author.IsBot == false
                && !pr.Labels.Any(label => config.Labels.BugLabels.Contains(label.Name) || config.Labels.MaintenanceLabels.Contains(label.Name))
                && !pr.Labels.Any(label => ContainsProjectLabel(label.Name, config))
        );
    }

    /// <summary>
    /// Gets the bug fix pull requests.
    /// </summary>
    /// <param name="pullRequests">The pull requests.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="projectLabel">The project label.</param>
    /// <returns>The bug fix pull requests.</returns>
    private static GitHubPullRequest[] GetBugFixPullRequests(GitHubPullRequest[] pullRequests, RootConfig config, string? projectLabel)
    {
        return projectLabel is null
            ? Array.FindAll(
                array: pullRequests,
                match: pr =>
                    pr.Author.IsBot == false
                    && pr.Labels.Any(label => config.Labels.BugLabels.Contains(label.Name))
            )
            : Array.FindAll(
                array: pullRequests,
                match: pr =>
                    pr.Author.IsBot == false
                    && pr.Labels.Any(label => config.Labels.BugLabels.Contains(label.Name))
                    && pr.Labels.Any(label => label.Name == projectLabel)
            );
    }

    /// <summary>
    /// Gets the other bug fix pull requests.
    /// </summary>
    /// <param name="pullRequests">The pull requests.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>All other bug fix pull requests.</returns>
    private static GitHubPullRequest[] GetOtherBugFixPullRequests(GitHubPullRequest[] pullRequests, RootConfig config)
    {
        return Array.FindAll(
            array: pullRequests,
            match: pr =>
                pr.Author.IsBot == false
                && pr.Labels.Any(label => config.Labels.BugLabels.Contains(label.Name))
                && !pr.Labels.Any(label => ContainsProjectLabel(label.Name, config))
        );
    }

    /// <summary>
    /// Gets the maintenance pull requests.
    /// </summary>
    /// <param name="pullRequests">The pull requests.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The maintenance pull requests.</returns>
    private static GitHubPullRequest[] GetMaintenancePullRequests(GitHubPullRequest[] pullRequests, RootConfig config)
    {
        return Array.FindAll(
            array: pullRequests,
            match: pr =>
                pr.Author.IsBot == false
                && pr.Labels.Any(label => config.Labels.MaintenanceLabels.Contains(label.Name) == true)
        );
    }

    /// <summary>
    /// Gets the dependency update pull requests.
    /// </summary>
    /// <param name="pullRequests">The pull requests.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The dependency update pull requests.</returns>
    private static GitHubPullRequest[] GetDependencyUpdatePullRequests(GitHubPullRequest[] pullRequests, RootConfig config)
    {
        return Array.FindAll(
            array: pullRequests,
            match: pr => pr.Author.Login == "app/dependabot"
        );
    }

    /// <summary>
    /// Determines if the project label is contained in the configuration.
    /// </summary>
    /// <param name="label">The label to check.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>Whether or not the label is in the configuration.</returns>
    private static bool ContainsProjectLabel(string label, RootConfig config)
    {
        return config.SeparateProjectLabel.ProjectLabels.Any(projectLabel => projectLabel.Label == label);
    }
}

using System.Diagnostics;
using GitHubReleaseGen.ConsoleApp.Models.GitHub;

namespace GitHubReleaseGen.ConsoleApp.Utilities;

/// <summary>
/// Utility methods for interacting with the GitHub CLI.
/// </summary>
public static class GhCliUtils
{
    /// <summary>
    /// Gets the merged pull requests for a repository.
    /// </summary>
    /// <returns>The merged pull requests for the repository.</returns>
    /// <exception cref="InvalidOperationException">An error occurred while running the 'gh pr list' s command.</exception>
    public static async Task<GitHubPullRequest[]> GetMergedPullRequests() => await GetMergedPullRequests(null, null, null);

    /// <summary>
    /// Gets the merged pull requests for a repository.
    /// </summary>
    /// <param name="repoOwner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="repoPath">The path to the local repository.</param>
    /// <returns>The merged pull requests for the repository.</returns>
    /// <exception cref="InvalidOperationException">An error occurred while running the 'gh pr list' s command.</exception>
    public static async Task<GitHubPullRequest[]> GetMergedPullRequests(string? repoOwner, string? repo, string? repoPath)
    {
        string[] processArgs = repoOwner is null && repo is null
            ? [ "pr", "list", "--state", "merged", "--json", "number,title,mergeCommit,author,labels" ]
            : [ "pr", "list", "--state", "merged", "--json", "number,title,mergeCommit,author,labels", "--repo", $"{repoOwner}/{repo}" ];
        
        ProcessStartInfo startInfo = new(
            fileName: "gh",
            arguments: processArgs
        )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            WorkingDirectory = repoPath ?? Environment.CurrentDirectory
        };

        Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start gh pr list process.");

        await process.WaitForExitAsync();

        GitHubPullRequest[] pullRequests = await JsonSerializer.DeserializeAsync(
            utf8Json: process.StandardOutput.BaseStream,
            jsonTypeInfo: GitHubApiJsonContext.Default.GitHubPullRequestArray
        ) ?? throw new InvalidOperationException("Failed to deserialize gh pr list.");

        return pullRequests;
    }

    /// <summary>
    /// Gets the URL of a repository.
    /// </summary>
    /// <returns>The URL of the repository.</returns>
    /// <exception cref="InvalidOperationException">An error occurred while running the 'gh repo view' command.</exception>
    public static async Task<string> GetRepoUrlAsync() => await GetRepoUrlAsync(null, null, null);

    /// <summary>
    /// Gets the URL of a repository.
    /// </summary>
    /// <param name="repoOwner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="repoPath">The path to the local repository.</param>
    /// <returns>The URL of the repository.</returns>
    /// <exception cref="InvalidOperationException">An error occurred while running the 'gh repo view' command.</exception>
    public static async Task<string> GetRepoUrlAsync(string? repoOwner, string? repo, string? repoPath)
    {
        string[] processArgs = repoOwner is null && repo is null
            ? [ "repo", "view", "--json", "url" ]
            : [ "repo", "view", "--json", "url", $"{repoOwner}/{repo}" ];
        
        ProcessStartInfo startInfo = new(
            fileName: "gh",
            arguments: processArgs
        )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            WorkingDirectory = repoPath ?? Environment.CurrentDirectory
        };

        Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start gh repo view process.");

        await process.WaitForExitAsync();

        GitHubRepo githubRepo = await JsonSerializer.DeserializeAsync(
            utf8Json: process.StandardOutput.BaseStream,
            jsonTypeInfo: GitHubApiJsonContext.Default.GitHubRepo
        ) ?? throw new InvalidOperationException("Failed to deserialize gh repo view.");
        
        return githubRepo.Url;
    }
}
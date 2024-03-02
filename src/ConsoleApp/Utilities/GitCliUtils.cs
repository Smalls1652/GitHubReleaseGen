using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitHubReleaseGen.ConsoleApp.Utilities;

/// <summary>
/// Utility methods for interacting with the Git CLI.
/// </summary>
public static partial class GitCliUtils
{
    private static ProcessStartInfo CreateGitProcessStartInfo(string[] arguments, string? workingDirectory)
    {
        return new(
            fileName: "git",
            arguments: arguments
        )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
        };
    }

    /// <summary>
    /// Gets the commits between two tags.
    /// </summary>
    /// <param name="baseTag">The base tag.</param>
    /// <returns>The commits between the two tags.</returns>
    /// <exception cref="InvalidOperationException">An error occurred while running the 'git log' command.</exception>
    public static async Task<List<string>> GetCommitsFromTagAsync(string baseTag) => await GetCommitsFromTagAsync(baseTag, null, null);

    /// <summary>
    /// Gets the commits between two tags.
    /// </summary>
    /// <param name="baseTag">The base tag.</param>
    /// <param name="newTag">The new tag.</param>
    /// <returns>The commits between the two tags.</returns>
    /// <exception cref="InvalidOperationException">An error occurred while running the 'git log' command.</exception>
    public static async Task<List<string>> GetCommitsFromTagAsync(string baseTag, string newTag) => await GetCommitsFromTagAsync(baseTag, newTag, null);

    /// <summary>
    /// Gets the commits between two tags.
    /// </summary>
    /// <param name="baseTag">The base tag.</param>
    /// <param name="newTag">The new tag.</param>
    /// <param name="repoPath">The path to the local repository.</param>
    /// <returns>The commits between the two tags.</returns>
    /// <exception cref="InvalidOperationException">An error occurred while running the 'git log' command.</exception>
    public static async Task<List<string>> GetCommitsFromTagAsync(string baseTag, string? newTag, string? repoPath)
    {
        if (!await VerifyTagExistsAsync(baseTag, repoPath))
        {
            throw new InvalidOperationException($"Tag '{baseTag}' does not exist.");
        }

        string resolvedNewTag;
        if (newTag is null)
        {
            resolvedNewTag = "HEAD";
        }
        else
        {
            if (!await VerifyTagExistsAsync(newTag, repoPath))
            {
                throw new InvalidOperationException($"Tag '{newTag}' does not exist.");
            }

            resolvedNewTag = newTag;
        }

        ProcessStartInfo processStartInfo = CreateGitProcessStartInfo(
            arguments: [
                "log",
                $"{baseTag}..{resolvedNewTag}",
                "--reverse",
                "--oneline"
            ],
            workingDirectory: repoPath
        );

        Process process = Process.Start(processStartInfo) ?? throw new InvalidOperationException("Failed to start git log process.");

        await process.WaitForExitAsync();

        string output = await process.StandardOutput.ReadToEndAsync();

        List<string> commits = [];
        foreach (Match matchItem in CommitRegex().Matches(output))
        {
            if (matchItem.Success && matchItem.Groups["commit"].Success)
            {
                commits.Add(matchItem.Groups["commit"].Value);
            }
        }

        return commits;
    }

    /// <summary>
    /// Verifies that a tag exists in a repository.
    /// </summary>
    /// <param name="tagName">The name of the tag to verify.</param>
    /// <returns><see langword="true"/> if the tag exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">An error occurred while running the 'git tag' command.</exception>
    public static async Task<bool> VerifyTagExistsAsync(string tagName) => await VerifyTagExistsAsync(tagName, null);

    /// <summary>
    /// Verifies that a tag exists in a repository.
    /// </summary>
    /// <param name="tagName">The name of the tag to verify.</param>
    /// <param name="repoPath">The path to the local repository.</param>
    /// <returns><see langword="true"/> if the tag exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">An error occurred while running the 'git tag' command.</exception>
    public static async Task<bool> VerifyTagExistsAsync(string tagName, string? repoPath)
    {
        ProcessStartInfo processStartInfo = CreateGitProcessStartInfo(
            arguments: [
                "tag",
                "--list",
                tagName
            ],
            workingDirectory: repoPath
        );

        Process process = Process.Start(processStartInfo) ?? throw new InvalidOperationException("Failed to start git tag process.");

        await process.WaitForExitAsync();

        string output = await process.StandardOutput.ReadToEndAsync();

        return output is not null && !string.IsNullOrWhiteSpace(output);
    }

    [GeneratedRegex(
        pattern: "(?'commit'[a-zA-z0-9]{7}) .+"
    )]
    internal static partial Regex CommitRegex();
}
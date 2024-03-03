using System.Diagnostics;
using System.Text.RegularExpressions;
using GitHubReleaseGen.ConsoleApp.Models.Git;

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
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
        };
    }

    private static async Task<Process> RunGitProcessAsync(ProcessStartInfo processStartInfo)
    {
        Process process = Process.Start(processStartInfo) ?? throw new InvalidOperationException("Failed to start git process.");

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            string errorOutput = await process.StandardError.ReadToEndAsync();

            throw new GitCliException("An error occurred while running the 'git' command.", errorOutput, GitCliExceptionType.Unknown);
        }

        return process;
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
        CommitInfo baseTagInfo;
        try
        {
            baseTagInfo = await GetCommitInfoAsync(baseTag, repoPath);
        }
        catch (Exception)
        {
            throw;
        }

        CommitInfo newTagInfo;
        try
        {
            newTagInfo = await GetCommitInfoAsync(newTag ?? "HEAD", repoPath);
        }
        catch (Exception)
        {
            throw;
        }

        ProcessStartInfo processStartInfo = CreateGitProcessStartInfo(
            arguments: [
                "log",
                $"{baseTagInfo.RefName}..{newTagInfo.RefName}",
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

    public static async Task<CommitInfo> GetCommitInfoAsync(string revision, string? repoPath)
    {
        ProcessStartInfo processStartInfo = CreateGitProcessStartInfo(
            arguments: [
                "log",
                "--no-patch",
                "--format='%h - %S - %s'",
                revision
            ],
            workingDirectory: repoPath
        );

        using Process process = await RunGitProcessAsync(processStartInfo);

        string output = await process.StandardOutput.ReadToEndAsync();

        return new(output);
    }

    [GeneratedRegex(
        pattern: "(?'commit'[a-zA-z0-9]{7}) .+"
    )]
    internal static partial Regex CommitRegex();
}
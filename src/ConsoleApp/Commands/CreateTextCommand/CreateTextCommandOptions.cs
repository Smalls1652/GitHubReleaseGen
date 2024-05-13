using System.CommandLine;

using GitHubReleaseGen.ConsoleApp.Models.Git;

namespace GitHubReleaseGen.ConsoleApp.Commands;

/// <summary>
/// Options for the 'create-text' command.
/// </summary>
public sealed class CreateTextCommandOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTextCommandOptions"/> class.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    public CreateTextCommandOptions(ParseResult parseResult)
    {
        BaseRef = ParseBaseRefArgument(parseResult);
        TargetRef = ParseTargetRefArgument(parseResult);
        RepoOwner = ParseRepoOwnerArgument(parseResult);
        RepoName = ParseRepoNameArgument(parseResult);
        LocalRepoPath = ParseLocalRepoPathArgument(parseResult);
        ExcludeOverviewSection = ParseExcludeOverviewSectionArgument(parseResult);
    }

    /// <summary>
    /// The base ref.
    /// </summary>
    public string BaseRef { get; set; }

    /// <summary>
    /// The target ref.
    /// </summary>
    public string TargetRef { get; set; }

    /// <summary>
    /// The repository owner.
    /// </summary>
    public string? RepoOwner { get; set; }

    /// <summary>
    /// The repository name.
    /// </summary>
    public string? RepoName { get; set; }

    /// <summary>
    /// The local repository path.
    /// </summary>
    public string LocalRepoPath { get; set; }

    /// <summary>
    /// Whether to exclude the overview section.
    /// </summary>
    public bool ExcludeOverviewSection { get; set; }

    /// <summary>
    /// Parse the '--base-ref' argument from the command line.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    /// <returns>The base ref.</returns>
    /// <exception cref="NullReferenceException">Thrown when the base ref was not provided.</exception>
    private static string ParseBaseRefArgument(ParseResult parseResult)
    {
        string baseRef = parseResult.GetValue<string>("--base-ref") ?? throw new NullReferenceException("Base ref is required.");

        return baseRef;
    }

    /// <summary>
    /// Parse the '--target-ref' argument from the command line.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    /// <returns>The target ref.</returns>
    /// <exception cref="NullReferenceException">Thrown when the target ref was not provided.</exception>
    private static string ParseTargetRefArgument(ParseResult parseResult)
    {
        string targetRef = parseResult.GetValue<string>("--target-ref") ?? throw new NullReferenceException("Target ref is required.");

        return targetRef;
    }

    /// <summary>
    /// Parse the '--repo-owner' argument from the command line.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    /// <returns>The repository owner.</returns>
    private static string? ParseRepoOwnerArgument(ParseResult parseResult)
    {
        string? repoOwner = parseResult.GetValue<string>("--repo-owner");

        return repoOwner;
    }

    /// <summary>
    /// Parse the '--repo-name' argument from the command line.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    /// <returns>The repository name.</returns>
    private static string? ParseRepoNameArgument(ParseResult parseResult)
    {
        string? repoName = parseResult.GetValue<string>("--repo-name");

        return repoName;
    }

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
    /// Parse the '--exclude-overview-section' argument from the command line.
    /// </summary>
    /// <param name="parseResult">The parse result from the command line.</param>
    /// <returns>Whether to exclude the overview section.</returns>
    private static bool ParseExcludeOverviewSectionArgument(ParseResult parseResult)
    {
        bool excludeOverviewSection = parseResult.GetValue<bool>("--exclude-overview-section");

        return excludeOverviewSection;
    }
}

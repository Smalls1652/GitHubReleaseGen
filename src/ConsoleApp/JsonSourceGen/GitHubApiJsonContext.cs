using GitHubReleaseGen.ConsoleApp.Models.GitHub;

namespace GitHubReleaseGen.ConsoleApp;

/// <summary>
/// Source generated JSON context for GitHub API classes.
/// </summary>
[JsonSerializable(typeof(GitHubPullRequest))]
[JsonSerializable(typeof(GitHubPullRequest[]))]
[JsonSerializable(typeof(GitHubUser))]
[JsonSerializable(typeof(GitHubRepo))]
internal partial class GitHubApiJsonContext : JsonSerializerContext
{ }

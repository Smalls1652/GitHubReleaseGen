namespace GitHubReleaseGen.ConsoleApp.Models.Git;

/// <summary>
/// Holds the tags in a Git repository.
/// </summary>
public sealed class GitTags
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GitTags"/> class.
    /// </summary>
    public GitTags()
    {
        Tags = GetTagsAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitTags"/> class.
    /// </summary>
    /// <param name="workingDirectory">The directory for the Git repository.</param>
    public GitTags(string workingDirectory)
    {
        Tags = GetTagsAsync(workingDirectory).GetAwaiter().GetResult();
    }

    /// <summary>
    /// The tags in the Git repository.
    /// </summary>
    public string[] Tags { get; set; } = [];

    /// <summary>
    /// Get the latest tag in the Git repository.
    /// </summary>
    /// <returns>The latest tag in the Git repository, otherwise null.</returns>
    public string? GetLatestTag()
    {
        if (Tags.Length == 0)
        {
            return null;
        }

        return Tags.Length == 1
            ? Tags[0]
            : Tags[1];
    }

    /// <summary>
    /// Get the latest tags in the Git repository.
    /// </summary>
    /// <param name="count">The last number of tags to get.</param>
    /// <returns>The latest tags in the Git repository.</returns>
    public string[] GetLatestTags(int count = 4)
    {
        if (Tags.Length == 1)
        {
            return Tags;
        }

        int tagCount = Tags.Length < count
            ? Tags.Length
            : count;

        return Tags[Range.EndAt(tagCount)];
    }

    /// <summary>
    /// Find tags in the Git repository.
    /// </summary>
    /// <param name="input">The input to search for in the tags.</param>
    /// <returns>The tags that contain the input.</returns>
    public string[] FindTags(string input)
    {
        return Array.FindAll(Tags, item => item.Contains(input));
    }

    /// <summary>
    /// Gets all tags in the Git repository.
    /// </summary>
    /// <returns>The all tags in the Git repository</returns>
    private static async Task<string[]> GetTagsAsync() => await GetTagsAsync(Environment.CurrentDirectory);

    /// <summary>
    /// Gets all tags in the Git repository.
    /// </summary>
    /// <param name="workingDirectory">The directory for the Git repository.</param>
    /// <returns>The all tags in the Git repository, otherwise null.</returns>
    private static async Task<string[]> GetTagsAsync(string workingDirectory)
    {
        string rootGitPath;

        try
        {
            RootGitDirectory rootGitDirectory = new(workingDirectory);

            rootGitPath = rootGitDirectory.Path;
        }
        catch
        {
            rootGitPath = workingDirectory;
        }

        // Get the tags in the Git repository
        using GitProcess gitProcess = new(
            arguments: [
                "tag",
                "--list",
                "--sort=-taggerdate"
            ],
            workingDirectory: rootGitPath
        );

        try
        {
            await gitProcess.RunGitProcessAsync();
        }
        catch
        {
            // If an error occurs, return an empty array.
            return [];
        }

        if (gitProcess.ProcessData is null)
        {
            // If the process data is null, return an empty array.
            return [];
        }

        string tagsOutput = await gitProcess.ProcessData.StandardOutput.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(tagsOutput))
        {
            // If the tags output is null or whitespace, return an empty array.
            return [];
        }

        string[] tags = tagsOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        if (tags.Length == 0)
        {
            // If there are no tags, return an empty array.
            return [];
        }

        return tags;
    }
}

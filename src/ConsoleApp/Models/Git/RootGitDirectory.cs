namespace GitHubReleaseGen.ConsoleApp.Models.Git;

/// <summary>
/// Holds the root directory of a Git repository.
/// </summary>
public sealed class RootGitDirectory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootGitDirectory"/> class.
    /// </summary>
    public RootGitDirectory()
    {
        Path = GetRootDirectory().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootGitDirectory"/> class.
    /// </summary>
    /// <param name="workingDirectory">The working directory.</param>
    public RootGitDirectory(string workingDirectory)
    {
        Path = GetRootDirectory(workingDirectory).GetAwaiter().GetResult();
    }

    /// <summary>
    /// The path to the root directory of the Git repository.
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// Gets the root directory of the Git repository.
    /// </summary>
    /// <returns>The path to the root directory.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Git process data is null.</exception>
    private async Task<string> GetRootDirectory() => await GetRootDirectory(Environment.CurrentDirectory);

    /// <summary>
    /// Gets the root directory of the Git repository.
    /// </summary>
    /// <param name="workingDirectory">The working directory.</param>
    /// <returns>The path to the root directory.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Git process data is null.</exception>
    private async Task<string> GetRootDirectory(string workingDirectory)
    {
        GitProcess gitProcess = new(
            arguments: [
                "rev-parse",
                "--show-toplevel"
            ],
            workingDirectory: workingDirectory
        );

        await gitProcess.RunGitProcessAsync();

        if (gitProcess.ProcessData is null)
        {
            throw new InvalidOperationException("Git process data is null.");
        }

        string rootDirectory = await gitProcess.ProcessData.StandardOutput.ReadToEndAsync();

        return rootDirectory.Trim();
    }
}
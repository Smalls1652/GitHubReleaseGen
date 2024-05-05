using System.Text;
using System.Text.RegularExpressions;

using GitHubReleaseGen.ConsoleApp.Models.GitHub;

namespace GitHubReleaseGen.ConsoleApp.Extensions.ReleaseText;

/// <summary>
/// Extension methods to create release text using <see cref="StringBuilder"/>.
/// </summary>
internal static partial class StringBuilderExtensions
{
    /// <summary>
    /// Adds the what's changed section to the release text.
    /// </summary>
    /// <param name="stringBuilder">The string builder.</param>
    /// <param name="pullRequests">The pull requests to add.</param>
    /// <param name="isSeparateProject">Whether the project is a separate project.</param>
    /// <returns>The string builder with the what's changed section added.</returns>
    public static StringBuilder AddWhatsChangedSection(this StringBuilder stringBuilder, GitHubPullRequest[] pullRequests, bool isSeparateProject)
    {
        if (pullRequests.Length != 0)
        {
            stringBuilder.AppendLine(!isSeparateProject ? "### ✍️ What's Changed\n" : "\n#### ✍️ What's Changed\n");

            foreach (var prItem in pullRequests)
            {
                stringBuilder.AppendLine($"* {prItem.Title} by @{prItem.Author.Login} in #{prItem.Number}");
            }
        }

        return stringBuilder;
    }

    /// <summary>
    /// Adds the bug fixes section to the release text.
    /// </summary>
    /// <param name="stringBuilder">The string builder.</param>
    /// <param name="pullRequests">The pull requests to add.</param>
    /// <param name="isSeparateProject">Whether the project is a separate project.</param>
    /// <returns>The string builder with the bug fixes section added.</returns>
    public static StringBuilder AddBugFixesSection(this StringBuilder stringBuilder, GitHubPullRequest[] pullRequests, bool isSeparateProject)
    {
        if (pullRequests.Length != 0)
        {
            stringBuilder.AppendLine(!isSeparateProject ? "### 🪳 Bug Fixes\n" : "\n#### 🪳 Bug Fixes\n");

            foreach (var prItem in pullRequests)
            {
                stringBuilder.AppendLine($"* {prItem.Title} by @{prItem.Author.Login} in #{prItem.Number}");
            }
        }

        return stringBuilder;
    }

    /// <summary>
    /// Adds the maintenance section to the release text. 
    /// </summary>
    /// <param name="stringBuilder">The string builder.</param>
    /// <param name="pullRequests">The pull requests to add.</param>
    /// <returns>The string builder with the maintenance section added.</returns>
    public static StringBuilder AddMaintenanceSection(this StringBuilder stringBuilder, GitHubPullRequest[] pullRequests)
    {
        if (pullRequests.Length != 0)
        {
            stringBuilder.AppendLine("\n### 🧹 Maintenance\n");

            foreach (var prItem in pullRequests)
            {
                stringBuilder.AppendLine($"* {prItem.Title} by @{prItem.Author.Login} in #{prItem.Number}");
            }
        }

        return stringBuilder;
    }

    /// <summary>
    /// Adds the dependency updates section to the release text.
    /// </summary>
    /// <param name="stringBuilder">The string builder.</param>
    /// <param name="pullRequests">The pull requests to add.</param>
    /// <returns>The string builder with the dependency updates section added.</returns>
    public static StringBuilder AddDependencyUpdatesSection(this StringBuilder stringBuilder, GitHubPullRequest[] pullRequests)
    {
        if (pullRequests.Length != 0)
        {
            stringBuilder.AppendLine("\n### ⛓️ Dependency updates\n");

            foreach (var prItem in pullRequests)
            {
                string dependencyUpdateTitle = PrettifyDependencyUpdateText(prItem.Title);
                stringBuilder.AppendLine($"* {dependencyUpdateTitle} by @{prItem.Author.Login.Replace("app/", "")} in #{prItem.Number}");
            }
        }

        return stringBuilder;
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

        Match dependencyUpdateMatch = DependencyUpdateRegex().Match(text);

        string modifiedText = text;

        if (dependencyUpdateMatch.Groups["dependencyName"].Success)
        {
            modifiedText = modifiedText.Replace(
                oldValue: dependencyUpdateMatch.Groups["dependencyName"].Value,
                newValue: $"**{dependencyUpdateMatch.Groups["dependencyName"].Value}**"
            );
        }

        if (dependencyUpdateMatch.Groups["previousVersion"].Success)
        {
            modifiedText = modifiedText.Replace(
                oldValue: dependencyUpdateMatch.Groups["previousVersion"].Value,
                newValue: $"`{dependencyUpdateMatch.Groups["previousVersion"].Value}`"
            );
        }

        if (dependencyUpdateMatch.Groups["newVersion"].Success)
        {
            modifiedText = modifiedText.Replace(
                oldValue: dependencyUpdateMatch.Groups["newVersion"].Value,
                newValue: $"`{dependencyUpdateMatch.Groups["newVersion"].Value}`"
            );
        }

        if (dependencyUpdateMatch.Groups["projectPath"].Success)
        {
            modifiedText = modifiedText.Replace(
                oldValue: dependencyUpdateMatch.Groups["projectPath"].Value,
                newValue: $"`{dependencyUpdateMatch.Groups["projectPath"].Value}`"
            );
        }

        return modifiedText;
    }

    [GeneratedRegex(
        pattern: "Bump (?'dependencyName'.+?) from (?'previousVersion'.+?) to (?'newVersion'.+?)(?>$| in (?'projectPath'.+))"
    )]
    internal static partial Regex DependencyUpdateRegex();
}
using System.CommandLine;
using GitHubReleaseGen.ConsoleApp;

CliConfiguration cliConfig = new(
    rootCommand: new RootCommand()
);

return await cliConfig.InvokeAsync(args);
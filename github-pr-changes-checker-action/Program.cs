using CommandLine;
using GithubPrChangesChecker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static CommandLine.Parser;

using IHost host = Host.CreateDefaultBuilder(args)
    // .ConfigureServices((_, services) => services.AddGitHubActionServices())
    .Build();

static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("DotNet.GitHubAction.Program")
            .LogError(
                string.Join(
                    Environment.NewLine, errors.Select(error => error.ToString())));

        Environment.Exit(2);
    });

await parser.WithParsedAsync(options => StartAnalysisAsync(options, host));
await host.RunAsync();

static async Task StartAnalysisAsync(ActionInputs inputs, IHost host)
{
    var updatedProjects = new[] { "raz", inputs.GithubToken[..4], "dwa" };

    Console.WriteLine($"::set-output name=updated-projects::{string.Join(';', updatedProjects)}");

    await Task.CompletedTask;

    Environment.Exit(0);
}
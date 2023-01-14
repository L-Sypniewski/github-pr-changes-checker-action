using System.Text;
using System.Text.Json;
using CommandLine;
using GithubPrChangesChecker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static CommandLine.Parser;

var currentEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

using var host = Host.CreateDefaultBuilder(args)
                     .UseEnvironment(currentEnvironment ?? Environments.Production)
                     .ConfigureAppConfiguration(builder =>
                     {
                         builder.SetBasePath(AppContext.BaseDirectory);
                         builder.AddJsonFile("appsettings.json", optional: false);
                         builder.AddJsonFile($"appsettings.{currentEnvironment}.json", optional: true);
                     })
                     .ConfigureServices(services =>
                     {
                         services.AddTransient<GithubClient>();
                         services.AddHttpClient<GithubClient>((serviceProvider, httpClient) =>
                         {
                             httpClient.BaseAddress = new Uri("https://api.github.com");
                             httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                             httpClient.DefaultRequestHeaders.Add("User-Agent", "product/1");
                         });
                     }).Build();

static TService Get<TService>(IHost host) where TService : notnull
{
    return host.Services.GetRequiredService<TService>();
}

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("GithubPrChangesChecker.Program")
            .LogError("Error: {Error}", string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
        Environment.Exit(2);
    });

await parser.WithParsedAsync(options => StartAnalysisAsync(options, host));

static async Task StartAnalysisAsync(ActionInputs inputs, IHost host)
{
    var hostEnv = host.Services.GetRequiredService<IHostEnvironment>();
    host.WaitForDebuggerToBeAttached(hostEnv.EnvironmentName, onCheck: () => Console.WriteLine("WaitForDebugger"));

    var client = Get<GithubClient>(host);
    var updatedProjects = await client.GetChangedProjectsNames(inputs.Owner, inputs.Name, inputs.PrNumber, inputs.GithubToken);
    var json = JsonSerializer.Serialize(updatedProjects);


    await SetOutputs(new KeyValuePair<string, string>("updated-projects", json));

    if (hostEnv.EnvironmentName == Environments.Development)
    {
        var logger = Get<ILoggerFactory>(host)
            .CreateLogger("GithubPrChangesChecker.Program");

        logger?.LogInformation("Outputs:\n'updated-projects': {Json}", json);
    }

    await Task.CompletedTask;
}

static Task SetOutputs(params KeyValuePair<string, string>[] outputValues)
{
    // Write GitHub Action workflow outputs.
    var gitHubOutputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
    if (string.IsNullOrWhiteSpace(gitHubOutputFile))
    {
        return Task.CompletedTask;
    }

    using StreamWriter textWriter = new(gitHubOutputFile, true, Encoding.UTF8);
    var setOutputTasks = outputValues
        .Select(kvp => textWriter.WriteLineAsync($"{kvp.Key}={kvp.Value}"))
        .ToArray();

    return Task.WhenAll(setOutputTasks);
}
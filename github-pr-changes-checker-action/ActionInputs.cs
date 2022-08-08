using CommandLine;

namespace GithubPrChangesChecker;

public class ActionInputs
{
    string _repositoryName = null!;

    [Option('o', "owner",
        Required = true,
        HelpText = "The owner, for example: \"dotnet\". Assign from `github.repository_owner`.")]
    public string Owner { get; set; } = null!;

    [Option('n', "name",
        Required = true,
        HelpText = "The repository name, for example: \"samples\". Assign from `github.repository`.")]
    public string Name
    {
        get => _repositoryName;
        set => ParseAndAssign(value, str => _repositoryName = str);
    }

    [Option('p', "pr_number",
        Required = true,
        HelpText = "The PR number, for example: \"123\". Assign from `github.event.pull_request.number`.")]
    public string PrNumber { get; set; }

    [Option('t', "github_token",
        Required = true,
        HelpText = "Github token. Assign from `secrets.GITHUB_TOKEN`.")]
    public string GithubToken { get; set; }
    static void ParseAndAssign(string? value, Action<string> assign)
    {
        if (value is { Length: > 0 } && assign is not null)
        {
            assign(value.Split("/")[^1]);
        }
    }
}
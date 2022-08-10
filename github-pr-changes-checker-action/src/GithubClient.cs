using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GithubPrChangesChecker;

public class GithubClient
{
    private readonly HttpClient _httpClient;

    public GithubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string[]> GetChangedProjectsNames(string owner, string name, string prNumber, string token)
    {
        AuthenticationHeaderValue authHeader = new("token", token);
        _httpClient.DefaultRequestHeaders.Authorization = authHeader;

        var requestUri = $"/repos/{owner}/{name}/pulls/{prNumber}/files";
        var uriWithParams = $"{requestUri}?per_page=100&page=1";
        var githubResponse = await _httpClient.GetFromJsonAsync<GithubFileChange[]>(uriWithParams);
        
        var projectNames = githubResponse?.Select(x => x.Filename.Split('/').First()).Distinct().ToArray();
        return projectNames ?? Array.Empty<string>();
    }
}
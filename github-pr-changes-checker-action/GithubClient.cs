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

    public async Task<GithubFileChange[]> GetResponse(string owner, string name, string prNumber, string token)
    {
        AuthenticationHeaderValue authHeader = new("token", token);
        _httpClient.DefaultRequestHeaders.Authorization = authHeader;

        var requestUri = $"/repos/{owner}/{name}/pulls/{prNumber}/files";
        var githubResponse = await _httpClient.GetFromJsonAsync<GithubFileChange[]>(requestUri);
        return githubResponse ?? Array.Empty<GithubFileChange>();
    }
}
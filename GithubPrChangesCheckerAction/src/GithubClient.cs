using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GithubPrChangesChecker;

public class GithubClient
{
    private readonly HttpClient _httpClient;

    public GithubClient(HttpClient httpClient) { _httpClient = httpClient; }

    public async Task<string[]> GetChangedProjectsNames(string owner, string name, string prNumber, string token)
    {
        ValidateParams(owner, name, prNumber, token);
        AuthenticationHeaderValue authHeader = new("token", token);
        _httpClient.DefaultRequestHeaders.Authorization = authHeader;

        var requestUri = $"/repos/{owner}/{name}/pulls/{prNumber}/files";

        var githubResponse = await GetFileChanges(_httpClient, requestUri);

        return githubResponse?
               .Select(x => x.Filename.Split('/').First())
               .Distinct()
               .ToArray() ?? Array.Empty<string>();
    }

    private static void ValidateParams(string owner, string name, string prNumber, string token)
    {
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("Parameter must not be null or empty", nameof(owner));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Parameter must not be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(prNumber))
            throw new ArgumentException("Parameter must not be null or empty", nameof(prNumber));
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Parameter must not be null or empty", nameof(token));
    }

    private static async Task<GithubFileChange[]> GetFileChanges(HttpClient httpClient, string requestUri)
    {
        var githubResponse = new List<GithubFileChange>();

        var pageNo = 1;
        while (true)
        {
            var uriWithParams = $"{requestUri}?per_page=100&page={pageNo}";

            var pageResults = await httpClient.GetFromJsonAsync<GithubFileChange[]>(uriWithParams);

            if (pageResults is null || pageResults.Length == 0)
            {
                break;
            }
            githubResponse.AddRange(pageResults);
            pageNo++;
        }

        return githubResponse.ToArray();
    }
}
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GithubPrChangesChecker;

public class GithubClient
{
    private readonly HttpClient _httpClient;

    public GithubClient(HttpClient httpClient) { _httpClient = httpClient; }

    public async Task<string[]> GetChangedProjectsNames(string owner, string name, string prNumber, string token)
    {
        AuthenticationHeaderValue authHeader = new("token", token);
        _httpClient.DefaultRequestHeaders.Authorization = authHeader;

        var requestUri = $"/repos/{owner}/{name}/pulls/{prNumber}/files";

        var githubResponse = await GetFileChanges(_httpClient, requestUri);

        return githubResponse?
               .Select(x => x.Filename.Split('/').First())
               .Distinct()
               .ToArray() ?? Array.Empty<string>();
    }

    private static async Task<GithubFileChange[]> GetFileChanges(HttpClient httpClient, string requestUri)
    {
        var githubResponse = new List<GithubFileChange>();

        var pageNo = 1;
        while (true)
        {
            var uriWithParams = $"{requestUri}?per_page=100&page={pageNo}";

            GithubFileChange[]? pageResults;
            try
            {
                pageResults = await httpClient.GetFromJsonAsync<GithubFileChange[]>(uriWithParams);
            }
            catch (HttpRequestException exception) when (exception.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("HTTP STATUS CODE " + exception.StatusCode);
                
                return Array.Empty<GithubFileChange>();
            }

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
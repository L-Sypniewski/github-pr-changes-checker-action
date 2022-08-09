using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GithubPrChangesChecker
{
    public class GithubClient
    {

        private readonly HttpClient _httpClient;

        public GithubClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GithubResponse[]> GetResponse(string owner, string name, string prNumber, string token)
        {
            AuthenticationHeaderValue authHeader = new("token", token);
            _httpClient.DefaultRequestHeaders.Authorization = authHeader;
            
            var requestUri = $"/repos/{owner}/{name}/pulls/{prNumber}/files";
            var githubResponses =  await _httpClient.GetFromJsonAsync<GithubResponse[]>(requestUri);
            return githubResponses ?? Array.Empty<GithubResponse>();
        }
    }
}
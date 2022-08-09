namespace GithubPrChangesChecker;

public record GithubResponse(string sha, string filename, string status, int additions, int deletions, int changes, string blob_url, string raw_url,
string contents_url, string patch);

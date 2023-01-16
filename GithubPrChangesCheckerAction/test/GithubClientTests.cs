using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GithubPrChangesChecker;
using Moq;
using Moq.Protected;
using Xunit;

namespace GithubPrChangesCheckerTests;

public class GithubClientTests
{
    private readonly GithubClient _sut;
    private readonly Mock<HttpMessageHandler> _messageHandlerMock;

    public GithubClientTests()
    {
        _messageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_messageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };
        _sut = new GithubClient(httpClient);
    }

    [Theory]
    [InlineData(null, "name", "prNumber", "token")]
    [InlineData("", "name", "prNumber", "token")]
    [InlineData("   ", "name", "prNumber", "token")]
    [InlineData("owner", null, "prNumber", "token")]
    [InlineData("owner", "", "prNumber", "token")]
    [InlineData("owner", "   ", "prNumber", "token")]
    [InlineData("owner", "   ", "prNumber", null)]
    [InlineData(null, null, null, null)]
    public async Task GetResponse_ThrowArgumentException_WhenProvidedParameterIsNullOrEmpty(string owner, string name, string prNumber, string token)
    {
        Func<Task> act = async () => await _sut.GetChangedProjectsNames(owner, name, prNumber, token);
        await act.Should().ThrowExactlyAsync<ArgumentException>();
    }


    [Fact]
    public async Task GetResponse_ReturnsProjectFolderName_WhenSingleFileWasChanged()
    {
        var content = GenerateTestData("MySingleProject/abc/def/code.cs");

        _messageHandlerMock.Protected()
                           .Setup<Task<HttpResponseMessage>>(
                               "SendAsync",
                               ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.EndsWith("page=1")),
                               ItExpr.IsAny<CancellationToken>()
                           ).ReturnsAsync(new HttpResponseMessage
                           {
                               StatusCode = HttpStatusCode.OK,
                               Content = new StringContent(content)
                           });
        _messageHandlerMock.Protected()
                           .Setup<Task<HttpResponseMessage>>(
                               "SendAsync",
                               ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.EndsWith("page=2")),
                               ItExpr.IsAny<CancellationToken>()
                           ).ReturnsAsync(new HttpResponseMessage
                           {
                               StatusCode = HttpStatusCode.OK,
                               Content = new StringContent("[]")
                           });


        var results = await _sut.GetChangedProjectsNames("abc", "abc", "abc", "abc");
        results.Should().BeEquivalentTo("MySingleProject");
    }

    [Fact]
    public async Task GetResponse_ReturnsDistinctProjectNames_WhenMoreThanOneFileWasModifiedInProject()
    {
        var content = GenerateTestData(
            "MySingleProject/abc/def/main.cs",
            "MySingleProject/abc/def/program.cs",
            "AnotherProject/abc/def/index.html",
            "AnotherProject/abc/def/styles.css");

        _messageHandlerMock.Protected()
                              .Setup<Task<HttpResponseMessage>>(
                                  "SendAsync",
                                  ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.EndsWith("page=1")),
                                  ItExpr.IsAny<CancellationToken>()
                              ).ReturnsAsync(new HttpResponseMessage
                              {
                                  StatusCode = HttpStatusCode.OK,
                                  Content = new StringContent(content)
                              });
        _messageHandlerMock.Protected()
                           .Setup<Task<HttpResponseMessage>>(
                               "SendAsync",
                               ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.EndsWith("page=2")),
                               ItExpr.IsAny<CancellationToken>()
                           ).ReturnsAsync(new HttpResponseMessage
                           {
                               StatusCode = HttpStatusCode.OK,
                               Content = new StringContent("[]")
                           });

        var results = await _sut.GetChangedProjectsNames("abc", "abc", "abc", "abc");
        results.Should().BeEquivalentTo("MySingleProject", "AnotherProject");
    }

    [Fact]
    public async Task GetResponse_ReturnsProjectNames_WhenResultsAreRetrievedFromMultipleApiPages()
    {
        var contentFromPage1 = GenerateTestData(
            "MySingleProject_page1/abc/def/main.cs",
            "MySingleProject_page1/abc/def/program.cs",
            "AnotherProject_page1/abc/def/index.html");

        var contentFromPage2 = GenerateTestData(
            "MySingleProject_page2/abc/def/main.cs",
            "SomeOther/def/program.cs",
            "AnotherProject_page2/abc/def/index.html");

        var contentFromPage3 = GenerateTestData(
            "AnotherProject_page3/abc/def/styles.css",
            "SuperSecretProjects_page3/abc/def/main.cs"
            );

        _messageHandlerMock.Protected()
                           .Setup<Task<HttpResponseMessage>>(
                               "SendAsync",
                               ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.EndsWith("page=1")),
                               ItExpr.IsAny<CancellationToken>()
                           ).ReturnsAsync(new HttpResponseMessage
                           {
                               StatusCode = HttpStatusCode.OK,
                               Content = new StringContent(contentFromPage1)
                           });
        _messageHandlerMock.Protected()
                           .Setup<Task<HttpResponseMessage>>(
                               "SendAsync",
                               ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.EndsWith("page=2")),
                               ItExpr.IsAny<CancellationToken>()
                           ).ReturnsAsync(new HttpResponseMessage
                           {
                               StatusCode = HttpStatusCode.OK,
                               Content = new StringContent(contentFromPage2)
                           });
        _messageHandlerMock.Protected()
                           .Setup<Task<HttpResponseMessage>>(
                               "SendAsync",
                               ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.EndsWith("page=3")),
                               ItExpr.IsAny<CancellationToken>()
                           ).ReturnsAsync(new HttpResponseMessage
                           {
                               StatusCode = HttpStatusCode.OK,
                               Content = new StringContent(contentFromPage3)
                           });
        _messageHandlerMock.Protected()
                           .Setup<Task<HttpResponseMessage>>(
                               "SendAsync",
                               ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.EndsWith("page=4")),
                               ItExpr.IsAny<CancellationToken>()
                           ).ReturnsAsync(new HttpResponseMessage
                           {
                               StatusCode = HttpStatusCode.OK,
                               Content = new StringContent("[]")
                           });

        var results = await _sut.GetChangedProjectsNames("abc", "abc", "abc", "abc");
        results.Should().BeEquivalentTo("MySingleProject_page1", "AnotherProject_page1", "MySingleProject_page2", "SomeOther", "AnotherProject_page2", "AnotherProject_page3", "SuperSecretProjects_page3");
    }

    [Fact]
    public async Task GetResponse_ThrowsHttpRequestException_WhenResponseIsNotFound()
    {
        _messageHandlerMock.Protected()
                             .Setup<Task<HttpResponseMessage>>(
                                 "SendAsync",
                                 ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.EndsWith("page=1")),
                                 ItExpr.IsAny<CancellationToken>()
                             ).ReturnsAsync(new HttpResponseMessage
                             {
                                 StatusCode = HttpStatusCode.NotFound
                             });

        Func<Task> act = async () => await _sut.GetChangedProjectsNames("abc", "abc", "abc", "abc");
        await act.Should().ThrowExactlyAsync<HttpRequestException>();
    }

    private static string GenerateTestData(params string[] filenames)
    {
        var fileChanges = filenames.Select(x => new { FileName = x }).ToArray();
        return JsonSerializer.Serialize(fileChanges);
    }
}
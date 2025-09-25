using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PlaylistGeneratorFunctionApp;

public class TrackInfo
{
    private readonly ILogger<TrackInfo> _logger;
    static readonly SpotifyAuthHelper authHelper = new(
        "dadcc3e1920f4fb78f62e6704e233a0f",
        "8d2958f6009b44a2ba45646d55c0c023",
        Environment.GetEnvironmentVariable("Refresh_token") // obtained during initial OAuth login
    );
    public TrackInfo(ILogger<TrackInfo> logger)
    {
        _logger = logger;
    }

    [Function("GetTrackInfo")]
    public async Task<IActionResult> GetTrackInfo([HttpTrigger(AuthorizationLevel.Function, "get", Route = "track/info")] HttpRequest req)
    {
        string accessToken = await authHelper.GetAccessTokenAsync();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"https://api.spotify.com/v1/tracks/4uLU6hMCjMI75M1A2tKUQC");
        string json = await response.Content.ReadAsStringAsync();

        Console.WriteLine(json);
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult(json);
    }
}
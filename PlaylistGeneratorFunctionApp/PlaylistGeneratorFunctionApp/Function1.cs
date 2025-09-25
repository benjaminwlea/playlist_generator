using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace PlaylistGeneratorFunctionApp;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    static readonly SpotifyAuthHelper authHelper = new(
        "dadcc3e1920f4fb78f62e6704e233a0f",
        "8d2958f6009b44a2ba45646d55c0c023",
        "AQAfWV4EtagD6U7kVzVPtlRn1mGFMvSRmEDdRxmrFgVmiEXI3eG-oC-KUQUJB1Vzphvsslp78v-KCQ8skdjO2PigmtQjQ5m9PsXISdmgB7wr0O9HipCySPnoiUDZdL1JMXA" // obtained during initial OAuth login
    );


    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("Function1")]
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

    [Function("Function2")]
    public static async Task<IActionResult> GetPlaylist(string id,[HttpTrigger(AuthorizationLevel.Function, "get", Route = "playlist/info")] HttpRequest req)
    {
        string accessToken = await authHelper.GetAccessTokenAsync();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"https://api.spotify.com/v1/playlists/"+id);
        string json = await response.Content.ReadAsStringAsync();

        Console.WriteLine(json);
        return new OkObjectResult(json);
    }
}
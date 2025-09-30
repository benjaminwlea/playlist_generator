using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistGeneratorFunctionApp
{

    public class PlaylistFunctions
    {
        private readonly ILogger<TrackInfo> _logger;
        static readonly SpotifyAuthHelper authHelper = new(
            Environment.GetEnvironmentVariable("Client-id"),
            Environment.GetEnvironmentVariable("Client-secret"),
            Environment.GetEnvironmentVariable("Refresh-token")// obtained during initial OAuth login
        );
        [Function("GetUsersPlaylists")]
        public static async Task<IActionResult> GetUsersPlaylists(string id, [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/playlists")] HttpRequest req)
        {
            string accessToken = await authHelper.GetAccessTokenAsync();

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            // Get the current user's playlists
            var response = await client.GetAsync("https://api.spotify.com/v1/me/playlists?limit=50");
            string json = await response.Content.ReadAsStringAsync();

            Console.WriteLine(json);
            return new OkObjectResult(json);
        }
    }
}

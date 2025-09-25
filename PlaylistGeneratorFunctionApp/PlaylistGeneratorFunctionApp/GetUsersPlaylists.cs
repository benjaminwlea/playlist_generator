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
        private readonly ILogger<Function1> _logger;
        static readonly SpotifyAuthHelper authHelper = new(
            "dadcc3e1920f4fb78f62e6704e233a0f",
            "8d2958f6009b44a2ba45646d55c0c023",
            Environment.GetEnvironmentVariable("Refresh_token")// obtained during initial OAuth login
        );
        [Function("Function3")]
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

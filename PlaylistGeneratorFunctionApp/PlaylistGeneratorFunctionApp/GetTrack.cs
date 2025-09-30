using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlaylistGeneratorFunctionApp
{
    public class TrackGet
    {
        static readonly SpotifyAuthHelper authHelper = new(
            Environment.GetEnvironmentVariable("Client-id"),
            Environment.GetEnvironmentVariable("Client-secret"),
            Environment.GetEnvironmentVariable("Refresh-token")// obtained during initial OAuth login
        );

        [Function("GetTrack")]
        public static async Task<IActionResult> GetTrack(string name,string artist, [HttpTrigger(AuthorizationLevel.Function, "get", Route = "track/id")] HttpRequest req)
        {
            string query = "track:" + name + " artist:" + artist;
            string accessToken = await authHelper.GetAccessTokenAsync();

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            string url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=1";

            var response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var item = doc.RootElement
                .GetProperty("tracks")
                .GetProperty("items")[0]
                .GetProperty("id").ToString();

            return new OkObjectResult(item);
        }

    }
}

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
      "dadcc3e1920f4fb78f62e6704e233a0f",
      "8d2958f6009b44a2ba45646d55c0c023",
      Environment.GetEnvironmentVariable("Refresh_token")// obtained during initial OAuth login
  );

        [Function("Function5")]
        public static async Task<IActionResult> GetPlaylist(string name,string artist, [HttpTrigger(AuthorizationLevel.Function, "get", Route = "track/id")] HttpRequest req)
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

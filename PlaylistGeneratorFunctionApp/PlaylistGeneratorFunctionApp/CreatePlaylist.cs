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
    public class MyClass
    {
        static readonly SpotifyAuthHelper authHelper = new(
            Environment.GetEnvironmentVariable("Client-id"),
            Environment.GetEnvironmentVariable("Client-secret"),
            Environment.GetEnvironmentVariable("Refresh-token") // obtained during initial OAuth login
        );

        [Function("CreatePlaylist")]
        public static async Task<IActionResult> CreatePlaylist(string listName, string accessToken, [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/playlists/create")] HttpRequest req)
        {
            //var accessToken = await authHelper.GetAccessTokenAsync();
            //var userId = "11899600"; // e.g., from GET /v1/me
            


            var playlistDescription = "Created by Playlist Generator";
            var isPublic = false;

            using var client = new HttpClient();

            // Add the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Get user ID
            var url = $"https://api.spotify.com/v1/me";
            var response = await client.GetAsync(url);
            string jsonUser = await response.Content.ReadAsStringAsync();
            var docUser = JsonDocument.Parse(jsonUser);

            string userId = docUser.RootElement.GetProperty("id").ToString();

            // Build the JSON payload
            var json = JsonSerializer.Serialize(new
            {
                name = listName,
                description = playlistDescription,
                @public = isPublic
            });

            // Wrap JSON string in StringContent with correct media type
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call the API
            url = $"https://api.spotify.com/v1/users/{userId}/playlists";
            response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Playlist created successfully!");
                Console.WriteLine(responseBody);
                return new OkObjectResult(responseBody);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to create playlist. Status: {response.StatusCode}");
                Console.WriteLine(errorBody);
                return new BadRequestObjectResult(errorBody);
            }

        }
    }
}

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
    public class TrackGenerator
    {
        static readonly SpotifyAuthHelper authHelper = new(
            Environment.GetEnvironmentVariable("Client-id"),
            Environment.GetEnvironmentVariable("Client-secret"),
            Environment.GetEnvironmentVariable("Refresh-token") // obtained during initial OAuth login
        );

        [Function("GenerateNewTracks")]
        public static async Task<IActionResult> GenerateNewTracks(string sourcePlaylistId, string generatedplaylistId, int numSongs, bool randomize, [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/playlists/generateNewTracks")] HttpRequest req)
        {
            Console.WriteLine($"sourcePlaylistId: {sourcePlaylistId}");
            Console.WriteLine($"generatedPlaylistId: {generatedplaylistId}");
            Console.WriteLine($"numSongs: {numSongs}");
            Console.WriteLine($"randomize: {randomize}");
            var accessToken = await authHelper.GetAccessTokenAsync();
            
            using var client = new HttpClient();

            // Add the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // TODO: Generating new songs using the Last.fm API


            string[] tracksToBeAdded = { "spotify:track:4c7jOHXUH9qDyMUVnWKUfR" };

            // Build the JSON payload
            var json = JsonSerializer.Serialize(new
            {
                uris = tracksToBeAdded
            });

            // Wrap JSON string in StringContent with correct media type
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call the API
            var url = $"https://api.spotify.com/v1/playlists/{generatedplaylistId}/tracks";
            //var url = $"https://api.spotify.com/v1/playlists/slong/tracks";
            var response = await client.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Tracks added successfully!");
                Console.WriteLine(responseBody);
                return new OkObjectResult(responseBody);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to add tracks. Status: {response.StatusCode}");
                Console.WriteLine(errorBody);
                return new BadRequestObjectResult(errorBody);
            }

        }
    }
}

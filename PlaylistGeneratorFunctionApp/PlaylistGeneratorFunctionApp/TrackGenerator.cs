using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.Collections;
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

            
            var responseSource = await client.GetAsync($"https://api.spotify.com/v1/playlists/" + sourcePlaylistId);
            string jsonSource = await responseSource.Content.ReadAsStringAsync();
            Console.WriteLine(jsonSource);
            var doc = JsonDocument.Parse(jsonSource);

            List<Track> sourcePlaylist = new List<Track>();

            foreach (JsonElement item in doc.RootElement.GetProperty("tracks").GetProperty("items").EnumerateArray())
            {
                List<string> names = new List<string>();
                foreach (JsonElement artist in item.GetProperty("track").GetProperty("artists").EnumerateArray())
                {
                    names.Add(artist.GetProperty("name").ToString());
                }
                string artists = String.Join(", ", names);
                sourcePlaylist.Add(new Track(item.GetProperty("track").GetProperty("name").ToString(),new Artist(artists)));
            }

            List<Track> generatedPlaylist = await PlaylistBuilder.GeneratePlaylist(sourcePlaylist, Environment.GetEnvironmentVariable("LastFmApiKey"), numSongs, randomize);
            List<string> trackURIs = new List<string>();

            foreach (Track t in generatedPlaylist)
            {
                Console.WriteLine(t);
                string urlNewTrack = $"https://api.spotify.com/v1/search?q=track:{t.Name} artist:{t.Artist.Name}&type=track&limit=1";

                var responseNewTrack = await client.GetAsync(urlNewTrack);
                string jsonNewTrack = await responseNewTrack.Content.ReadAsStringAsync();
                var docNewTrack = JsonDocument.Parse(jsonNewTrack);

                string uri = docNewTrack.RootElement
                    .GetProperty("tracks")
                    .GetProperty("items")[0]
                    .GetProperty("uri").ToString();
                trackURIs.Add(uri);
            }

            
            // Build the JSON payload
            var json = JsonSerializer.Serialize(new
            {
                uris = trackURIs.ToArray()
            });

            // Wrap JSON string in StringContent with correct media type
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call the API
            var url = $"https://api.spotify.com/v1/playlists/{generatedplaylistId}/tracks";
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Drawing.Printing;

namespace PlaylistGeneratorFunctionApp;

public static class PlaylistBuilder
{
    private const string rootURL = "http://ws.audioscrobbler.com/2.0/?";

    public static async Task<List<Track>> GeneratePlaylist(List<Track> playlist, string apiKey, int numSongs = 50, bool randomizeNewList = true)
    {
        List<Track> generatedPlaylist = new List<Track>();

        int newSongsPerSource = (int)Math.Ceiling((decimal)numSongs / playlist.Count);

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Playlist Generator/0.1");

        foreach (Track track in playlist)
        {
            string url = rootURL;
            url += $"method=track.getsimilar&track={track.Name}&artist={track.Artist.Name}";
            url += $"&api_key={apiKey}&format=json";

            // TODO: Write better exception and error handling.
            try
            {
                var response = await client.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<LastfmTrackGetSimilarResponse>(json, options);
                Console.WriteLine($"newSongsPerSource {newSongsPerSource}");
                Console.WriteLine($"data.SimilarTracks.Track.Count {data.SimilarTracks.Track.Count}" );
                int newSongs = Math.Min(newSongsPerSource, data.SimilarTracks.Track.Count);
                if (newSongs > 0)
                {
                    generatedPlaylist.AddRange(data.SimilarTracks.Track.GetRange(0, newSongs));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}: {ex.Message}\n");
                Console.WriteLine($"URL used: {url}");
                return new List<Track>();
            }

            await Task.Delay(200);
        }

        if (randomizeNewList)
        {
            var arr = generatedPlaylist.ToArray();
            Random.Shared.Shuffle(arr);
            generatedPlaylist.Clear();
            generatedPlaylist.AddRange(arr);
        }

        return generatedPlaylist.GetRange(0, Math.Min(numSongs,generatedPlaylist.Count()));



    }
}
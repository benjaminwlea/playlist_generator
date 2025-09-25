using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class SpotifyAuthHelper
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _refreshToken;

    public SpotifyAuthHelper(string clientId, string clientSecret, string refreshToken)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _refreshToken = refreshToken;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        using var client = new HttpClient();

        // Create Basic Auth header
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var body = new StringContent(
            $"grant_type=refresh_token&refresh_token={_refreshToken}",
            Encoding.UTF8,
            "application/x-www-form-urlencoded"
        );

        var response = await client.PostAsync("https://accounts.spotify.com/api/token", body);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to refresh token: {response.StatusCode} - {json}");
        }

        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString();
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public static class AuthCallback
{
    private static readonly HttpClient httpClient = new HttpClient();

    [Function("AuthCallback")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/callback")] HttpRequest req)
    {
        // Read request body (code + verifier from frontend)
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<AuthRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (data == null || string.IsNullOrEmpty(data.Code) || string.IsNullOrEmpty(data.Verifier))
        {
            return new BadRequestObjectResult("Missing code or verifier");
        }

        // Load config from environment variables
        string clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
        string redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI");
        Console.WriteLine($"SPOTIFY_REDIRECT_URI {redirectUri}");

        // Build request to Spotify
        var body = new Dictionary<string, string>
        {
            {"client_id", clientId},
            {"grant_type", "authorization_code"},
            {"code", data.Code},
            {"redirect_uri", redirectUri},
            {"code_verifier", data.Verifier}
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Content = new FormUrlEncodedContent(body)
        };

        var response = await httpClient.SendAsync(requestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new BadRequestObjectResult($"Spotify error: {responseContent}");
        }

        // Pass the access_token back to frontend
        return new OkObjectResult(JsonDocument.Parse(responseContent));
    }

    private class AuthRequest
    {
        public string Code { get; set; }
        public string Verifier { get; set; }
    }
}

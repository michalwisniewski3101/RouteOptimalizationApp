using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace YourAppNamespace.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        // Endpoint to initiate OAuth flow
        [HttpGet("login")]
        public IActionResult Login()
        {
            var clientId = _configuration["GoogleOAuth:ClientId"];
            var redirectUri = Url.Action("Callback", "Auth", null, Request.Scheme);

            var authorizationUrl = $"https://accounts.google.com/o/oauth2/auth?" +
                $"client_id={clientId}&" +
                $"redirect_uri={redirectUri}&" +
                $"response_type=code&" +
                $"scope=https://www.googleapis.com/auth/cloud-platform&" +
                $"access_type=offline";

            return Redirect(authorizationUrl);
        }

        // Callback for OAuth response
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code is missing.");
            }

            var clientId = _configuration["GoogleOAuth:ClientId"];
            var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
            var redirectUri = Url.Action("Callback", "Auth", null, Request.Scheme);

            var tokenRequestParams = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var httpClient = _httpClientFactory.CreateClient();
            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequestParams);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)tokenResponse.StatusCode, "Error exchanging code for token.");
            }

            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JObject.Parse(tokenResponseContent);

            var accessToken = tokenData["access_token"]?.ToString();
            var refreshToken = tokenData["refresh_token"]?.ToString();
            var expiresIn = tokenData["expires_in"]?.ToString();

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn
            });
        }
    }
}

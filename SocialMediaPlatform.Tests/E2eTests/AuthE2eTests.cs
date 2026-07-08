using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SocialMediaPlatform.Tests.E2eSupport;

namespace SocialMediaPlatform.Tests.E2eTests;


// IClassFixture<ApiFactory>: ONE app + ONE database is shared by all tests in
// this class (booting the host is expensive). Tests therefore use unique
// usernames/emails so they don't collide inside the shared database.

public class AuthE2eTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _app;

    public AuthE2eTests(ApiFactory app) => _app = app;

    // Helper: register a fresh user through the real endpoint and return the JSON body.
    private static async Task<JsonElement> RegisterAsync(HttpClient client, string tag)
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"{tag}_{unique}@example.com",
            username = $"{tag}_{unique}",
            fullName = $"{tag} user",
            password = "P@ssw0rd1"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "registration must succeed before the rest of the journey can run");
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task Register_ReturnsSuccessAndUsableToken()
    {
        // Arrange
        var client = _app.CreateHttpsClient();

        // Act: sign up through the real endpoint (JSON body → [FromBody] RegisterRequest).
        var body = await RegisterAsync(client, "reg");

        // Assert — this pins the SERIALIZATION CONTRACT the frontend depends on:
        // camelCase property names and the fields' presence. If someone renamed
        // AuthenticationResult.Token or changed the casing policy, this breaks.
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("userName").GetString().Should().StartWith("reg_");
    }

    [Fact]
    public async Task Login_WithRegisteredCredentials_ReturnsToken()
    {
        // Arrange: a registered user (via HTTP — the real Identity stack hashed the
        // password and stored NormalizedEmail; no hand-seeded rows here).
        var client = _app.CreateHttpsClient();
        var registered = await RegisterAsync(client, "login");
        var email = registered.GetProperty("userName").GetString() + "@example.com";
        // (register helper builds email as "<username>@example.com")

        // Act: log in with the same credentials.
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "P@ssw0rd1"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns400()
    {
        // Arrange
        var client = _app.CreateHttpsClient();
        var registered = await RegisterAsync(client, "wrongpw");
        var email = registered.GetProperty("userName").GetString() + "@example.com";

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "definitely-wrong"
        });

        // Assert: the controller maps a failed AuthenticationResult to 400.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        // Arrange
        var client = _app.CreateHttpsClient();

        // Act: hit an [Authorize]-protected route with no Authorization header.
        var response = await client.GetAsync("/api/posts/feed");

        // Assert: the JWT middleware challenges , the request never reaches the
        // controller. No layer below E2E can verify this: unit/integration tests
        // call service methods directly and bypass [Authorize] entirely.
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithToken_Returns200()
    {
        // Arrange: register → take the token → present it as a Bearer header.
        var client = _app.CreateHttpsClient();
        var body = await RegisterAsync(client, "feed");
        var token = body.GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act: same protected route, now authenticated.
        var response = await client.GetAsync("/api/posts/feed");

        // Assert: the FULL chain worked , TokenService signed a token that the
        // middleware's TokenValidationParameters accepted (issuer/audience/key
        // symmetry), the NameIdentifier claim parsed into CurrentUserId, and the
        // controller returned the (empty) feed.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

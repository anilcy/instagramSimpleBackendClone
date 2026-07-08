using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SocialMediaPlatform.Tests.E2eSupport;

namespace SocialMediaPlatform.Tests.E2eTests;

// ─────────────────────────────────────────────────────────────────────────────
// Clients build behavior on top of status codes (retry on 5xx, show a form error
// on 400, show "no access" on 403). That mapping lives in GlobalExceptionHandlingMiddleware 


public class ErrorContractE2eTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _app;

    public ErrorContractE2eTests(ApiFactory app) => _app = app;

    // Register a user and return an HttpClient already carrying their Bearer token.
    private async Task<HttpClient> CreateAuthenticatedClientAsync(string tag)
    {
        var client = _app.CreateHttpsClient();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"{tag}_{unique}@example.com",
            username = $"{tag}_{unique}",
            fullName = $"{tag} user",
            password = "P@ssw0rd1"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // Create a post and return its id. This is a real multipart/form-data request —
    // the same wire format the frontend sends: a text part ("Caption") and a file
    // part ("MediaFiles", required by PostCreateDto). The fake IFileStorageService
    // swallows the upload, but model binding of the file part is fully real.
    private static async Task<Guid> CreatePostAsync(HttpClient client, string caption)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(caption), "Caption");
        var file = new ByteArrayContent(new byte[] { 1, 2, 3 });
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(file, "MediaFiles", "photo.jpg");
        var response = await client.PostAsync("/api/posts", form);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task EditingSomeoneElsesPost_Returns403_WithProblemJson()
    {
        // Arrange: A owns a post; B is logged in.
        var clientA = await CreateAuthenticatedClientAsync("owner");
        var clientB = await CreateAuthenticatedClientAsync("intruder");
        var postId = await CreatePostAsync(clientA, "A's post");

        // Act: B tries to edit A's post.
        var response = await clientB.PutAsJsonAsync($"/api/posts/{postId}", new { caption = "hacked" });

        // Assert: UnauthorizedAccessException from the service is translated by the
        // middleware into 403 + application/problem+json , the contract clients see.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("detail").GetString().Should().Be("You can only edit your own posts.");
    }

    [Fact]
    public async Task GettingNonexistentPost_Returns404()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("reader");

        // Act: ask for a post that does not exist.
        var response = await client.GetAsync($"/api/posts/{Guid.NewGuid()}");

        // Assert: 404 — an absent resource is the CLIENT asking for something that
        // isn't there (4xx), not the SERVER failing (5xx)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RegisteringWithInvalidBody_Returns400_FromAutomaticModelValidation()
    {
        // Arrange
        var client = _app.CreateHttpsClient();

        // Act: required fields missing , [ApiController] rejects this BEFORE any of
        // our code runs (automatic model validation), another pipeline-only behavior.
        var response = await client.PostAsync("/api/auth/register",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

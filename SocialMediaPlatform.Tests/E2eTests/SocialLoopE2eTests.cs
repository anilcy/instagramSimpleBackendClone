using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SocialMediaPlatform.Tests.E2eSupport;

namespace SocialMediaPlatform.Tests.E2eTests;
// The story: Ayşe posts a photo. Burak follows Ayşe, sees the post in his feed,
// and likes it. Ayşe gets notified. One request exactly like the frontend would.

public class SocialLoopE2eTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _app;

    public SocialLoopE2eTests(ApiFactory app) => _app = app;

    private async Task<(HttpClient Client, Guid UserId)> CreateAuthenticatedClientAsync(string tag)
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
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body.GetProperty("token").GetString());
        return (client, body.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task SocialLoop_PostFollowFeedLikeNotification()
    {
        // Arrange: two real users, logged in over HTTP.
        var (clientAyse, ayseId) = await CreateAuthenticatedClientAsync("ayse");
        var (clientBurak, burakId) = await CreateAuthenticatedClientAsync("burak");

        // Step 1: Ayşe posts a photo (multipart/form-data, like the real app).
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("sunset 🌅"), "Caption");
        var photo = new ByteArrayContent(new byte[] { 1, 2, 3 });
        photo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        form.Add(photo, "MediaFiles", "sunset.jpg");
        var createResponse = await clientAyse.PostAsync("/api/posts", form);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // The created post's id, from the JSON the frontend would receive.
        // ASP.NET serializes C# PascalCase properties as camelCase JSON, and
        // GetProperty is case-sensitive.
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var postId = created.GetProperty("id").GetGuid();

        //  Step 2: Burak follows Ayşe (public account → follow is instantly accepted).
        var followResponse = await clientBurak.PostAsync($"/api/follows/{ayseId}", null);
        followResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //  Step 3: BURAK's feed now shows Ayşe's post , the actor must be Burak.
        var feedResponse = await clientBurak.GetAsync("/api/posts/feed");
        feedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var feed = await feedResponse.Content.ReadFromJsonAsync<JsonElement>();
        feed.EnumerateArray()
            .Any(p => p.GetProperty("id").GetGuid() == postId)
            .Should().BeTrue("the post of a followed user must appear in the follower's feed");

        // Step 4: Burak likes the post.
        var likeResponse = await clientBurak.PostAsync($"/api/posts/{postId}/like", null);
        likeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //  Step 5: Ayşe's notifications include the like on her post.
        var notificationsResponse = await clientAyse.GetAsync("/api/notifications");
        notificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await notificationsResponse.Content.ReadFromJsonAsync<JsonElement>();
        notifications.EnumerateArray()
            .Any(n => n.TryGetProperty("postId", out var pid)
                      && pid.ValueKind == JsonValueKind.String
                      && pid.GetGuid() == postId)
            .Should().BeTrue("liking someone's post must notify its author");
    }
}

using System.Text;
using SocialMediaPlatform.Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace SocialMediaPlatform.Tests.UnitTestSupport;

public static class TestHelpers
{
    public static Mock<UserManager<AppUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    public static IFormFile CreateFormFile(string fileName = "image.jpg", byte[]? content = null)
    {
        var bytes = content ?? Encoding.UTF8.GetBytes("fake image content");
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }

    public static DefaultHttpContext CreateHttpContext(Guid? userId = null, string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.TraceIdentifier = "trace-123";

        if (userId.HasValue)
        {
            context.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.Value.ToString()) },
                    "TestAuth"));
        }

        return context;
    }
}

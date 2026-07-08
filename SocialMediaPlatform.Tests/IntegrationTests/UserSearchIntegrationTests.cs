using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Data.Repositories;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.IntegrationSupport;

namespace SocialMediaPlatform.Tests.IntegrationTests;


public class UserSearchIntegrationTests : IntegrationTestBase
{
    private async Task<AppUser> SeedNamedUserAsync(string userName, string? fullName)
    {
        var user = new AppUser(userName, $"{Guid.NewGuid():N}@example.com", fullName);
        await using var ctx = CreateContext();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();
        return user;
    }

    // Real life: someone types "ali" into the search box. They obviously expect to
    // find the user "Ali_Yilmaz" — nobody types the exact casing of a username.
    [Fact]
    public async Task Search_IsCaseInsensitive_LowercaseTermFindsCapitalizedUser()
    {
        // Arrange: a user whose name is stored capitalized, plus an unrelated user
        // (the control — proves we're matching, not just returning everyone).
        var ali = await SeedNamedUserAsync("Ali_Yilmaz", "Ali Yilmaz");
        await SeedNamedUserAsync("Mehmet_Demir", "Mehmet Demir");

        // Act: search with a lowercase term, like a real person would.
        await using var ctx = CreateContext();
        var results = await new UserRepository(ctx).SearchUsersAsync("ali", 1, 20);

        // Assert: Ali is found despite the casing difference; Mehmet is not.
        results.Should().ContainSingle(u => u.Id == ali.Id);
    }
}

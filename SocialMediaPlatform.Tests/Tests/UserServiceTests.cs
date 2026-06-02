using AutoMapper;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.TestSupport;
using Microsoft.AspNetCore.Identity;

namespace SocialMediaPlatform.Tests.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
    private readonly Mock<IFollowRepository> _followRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly Mock<UserManager<AppUser>> _userManager = TestHelpers.CreateUserManagerMock();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository.Object, _followRepository.Object, _mapper.Object, _userManager.Object);
    }

    [Fact]
    public async Task GetUserProfileAsync_ShouldAggregateCountsAndRelationshipData()
    {
        var userId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var user = new AppUser { Id = userId, UserName = "ani", FullName = "Anil Y" };
        var expected = new UserDto
        {
            Id = userId,
            UserName = user.UserName,
            FullName = user.FullName,
            PostsCount = 12,
            FollowersCount = 34,
            FollowingCount = 56,
            IsFollowing = true,
            FollowStatus = FollowStatus.Accepted
        };

        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepository.Setup(r => r.GetUserPostsCountAsync(userId)).ReturnsAsync(12);
        _followRepository.Setup(r => r.GetFollowersCountAsync(userId)).ReturnsAsync(34);
        _followRepository.Setup(r => r.GetFollowingCountAsync(userId)).ReturnsAsync(56);
        _followRepository.Setup(r => r.IsFollowingAsync(currentUserId, userId)).ReturnsAsync(true);
        _followRepository.Setup(r => r.GetFollowRelationshipAsync(currentUserId, userId)).ReturnsAsync(new Follow { Status = FollowStatus.Accepted });
        _mapper.Setup(m => m.Map<UserDto>(user)).Returns(expected);

        var result = await _sut.GetUserProfileAsync(userId, currentUserId);

        result.Should().BeEquivalentTo(expected);
        _userRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _followRepository.Verify(r => r.IsFollowingAsync(currentUserId, userId), Times.Once);
        _followRepository.Verify(r => r.GetFollowRelationshipAsync(currentUserId, userId), Times.Once);
    }

    [Fact]
    public async Task GetUserProfileAsync_ShouldThrowWhenUserMissing()
    {
        var userId = Guid.NewGuid();

        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((AppUser?)null);

        var act = async () => await _sut.GetUserProfileAsync(userId);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("User not found");
    }

    [Fact]
    public async Task GetUserByUserNameAsync_ShouldReturnProfileForUsername()
    {
        var userId = Guid.NewGuid();
        var userName = "ani";
        var user = new AppUser { Id = userId, UserName = userName, FullName = "Anil Y" };
        var expected = new UserDto
        {
            Id = userId,
            UserName = userName,
            FullName = "Anil Y",
            PostsCount = 1,
            FollowersCount = 2,
            FollowingCount = 3
        };

        _userRepository.Setup(r => r.GetUserByUserNameAsync(userName)).ReturnsAsync(user);
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepository.Setup(r => r.GetUserPostsCountAsync(userId)).ReturnsAsync(1);
        _followRepository.Setup(r => r.GetFollowersCountAsync(userId)).ReturnsAsync(2);
        _followRepository.Setup(r => r.GetFollowingCountAsync(userId)).ReturnsAsync(3);
        _mapper.Setup(m => m.Map<UserDto>(user)).Returns(expected);

        var result = await _sut.GetUserByUserNameAsync(userName);

        result.Should().BeEquivalentTo(expected);
        _userRepository.Verify(r => r.GetUserByUserNameAsync(userName), Times.Once);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_ShouldUpdateAndReturnFreshProfile()
    {
        var userId = Guid.NewGuid();
        var user = new AppUser
        {
            Id = userId,
            UserName = "ani",
            FullName = "Old Name",
            Bio = "old bio",
            WebsiteUrl = "https://old.example",
            IsPrivate = false
        };
        var dto = new UserProfileUpdateDto
        {
            FullName = "New Name",
            Bio = "new bio",
            WebsiteUrl = "https://new.example",
            IsPrivate = true
        };
        var expected = new UserDto
        {
            Id = userId,
            UserName = user.UserName,
            FullName = dto.FullName,
            Bio = dto.Bio,
            WebsiteUrl = dto.WebsiteUrl,
            IsPrivate = dto.IsPrivate,
            PostsCount = 9,
            FollowersCount = 8,
            FollowingCount = 7
        };

        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepository.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
        _userRepository.Setup(r => r.GetUserPostsCountAsync(userId)).ReturnsAsync(9);
        _followRepository.Setup(r => r.GetFollowersCountAsync(userId)).ReturnsAsync(8);
        _followRepository.Setup(r => r.GetFollowingCountAsync(userId)).ReturnsAsync(7);
        _mapper.Setup(m => m.Map<UserDto>(user)).Returns(expected);

        var result = await _sut.UpdateUserProfileAsync(userId, dto);

        result.Should().BeEquivalentTo(expected);
        user.FullName.Should().Be(dto.FullName);
        user.Bio.Should().Be(dto.Bio);
        user.WebsiteUrl.Should().Be(dto.WebsiteUrl);
        user.IsPrivate.Should().Be(dto.IsPrivate);
        user.UpdatedAt.Should().NotBe(default);
        _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task SearchUsersAsync_ShouldMapSearchResults()
    {
        var term = "ani";
        var users = new List<AppUser>
        {
            new() { Id = Guid.NewGuid(), UserName = "ani1", FullName = "Ani One" },
            new() { Id = Guid.NewGuid(), UserName = "ani2", FullName = "Ani Two" }
        };
        var expected = new List<UserSummaryDto>
        {
            new() { Id = users[0].Id, UserName = users[0].UserName!, FullName = users[0].FullName },
            new() { Id = users[1].Id, UserName = users[1].UserName!, FullName = users[1].FullName }
        };

        _userRepository.Setup(r => r.SearchUsersAsync(term, 1, 20)).ReturnsAsync(users);
        _mapper.Setup(m => m.Map<List<UserSummaryDto>>(users)).Returns(expected);

        var result = await _sut.SearchUsersAsync(term);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_ShouldDelegateToRepository()
    {
        var userId = Guid.NewGuid();

        _userRepository.Setup(r => r.UpdateLastLoginAsync(userId)).Returns(Task.CompletedTask);

        await _sut.UpdateLastLoginAsync(userId);

        _userRepository.Verify(r => r.UpdateLastLoginAsync(userId), Times.Once);
    }
}

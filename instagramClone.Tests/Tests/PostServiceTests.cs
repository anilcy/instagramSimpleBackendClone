/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using instagramClone.Business.Interfaces;
using instagramClone.Business.Services;
using instagramClone.Data.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace instagramClone.Tests.Services
{
    public class PostServiceTests
    {
        private readonly Mock<IPostRepository> _postRepo;
        private readonly Mock<IFileStorageService> _fileStorage;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<UserManager<AppUser>> _userManager;
        private readonly Mock<IPrivacyService> _privacy;
        private readonly PostService _sut; // SUT = System Under Test

        public PostServiceTests()
        {
            _postRepo = new Mock<IPostRepository>(MockBehavior.Strict);
            _fileStorage = new Mock<IFileStorageService>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Strict);
            _privacy = new Mock<IPrivacyService>(MockBehavior.Strict);

            // UserManager needs a store; we can pass a mock store + nulls for other deps
            var store = new Mock<IUserStore<AppUser>>();
            _userManager = new Mock<UserManager<AppUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!
            );

            _sut = new PostService(
                _postRepo.Object,
                _fileStorage.Object,
                _mapper.Object,
                _userManager.Object,
                _privacy.Object
            );
        }

        private static IFormFile FakeFormFile(string name = "image.jpg")
        {
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            return new FormFile(stream, 0, stream.Length, "file", name);
        }

        [Fact]
        public async Task CreatePostAsync_ShouldUploadFile_PersistPost_MapAndReturnDto()
        {
            // Arrange
            var dto = new PostCreateDto
            {
                Caption = "hello",
                ImageFile = FakeFormFile()
            };
            var userId = Guid.NewGuid();
            var uploadedUrl = "https://cdn/img.jpg";

            _fileStorage.Setup(s => s.UploadFileAsync(dto.ImageFile))
                        .ReturnsAsync(uploadedUrl);

            _postRepo.Setup(r => r.InsertAsync(It.IsAny<Post>()))
                     .Returns(Task.CompletedTask);
            _postRepo.Setup(r => r.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            _userManager.Setup(u => u.FindByIdAsync(userId.ToString()))
                        .ReturnsAsync(new AppUser { Id = userId, FullName = "Test User" });

            var mapped = new PostDto { Id = 123, Caption = dto.Caption, ImageUrl = uploadedUrl };
            _mapper.Setup(m => m.Map<PostDto>(It.IsAny<Post>()))
                   .Returns(mapped);

            // Act
            var result = await _sut.CreatePostAsync(dto, userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(123);
            result.ImageUrl.Should().Be(uploadedUrl);
            result.Caption.Should().Be("hello");

            _fileStorage.Verify(s => s.UploadFileAsync(dto.ImageFile), Times.Once);
            _postRepo.Verify(r => r.InsertAsync(It.Is<Post>(p =>
                p.AuthorId == userId &&
                p.ImageUrl == uploadedUrl &&
                p.Caption == "hello" &&
                p.IsDeleted == false
            )), Times.Once);
            _postRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _userManager.Verify(u => u.FindByIdAsync(userId.ToString()), Times.Once);
            _mapper.Verify(m => m.Map<PostDto>(It.IsAny<Post>()), Times.Once);
        }

        [Fact]
        public async Task GetPostsAsync_ShouldCheckPrivacy_ThenMapList()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var posts = new List<Post>
            {
                new Post { Id = 1, AuthorId = targetUserId, ImageUrl = "u1" },
                new Post { Id = 2, AuthorId = targetUserId, ImageUrl = "u2" }
            };

            _privacy.Setup(p => p.EnsureCanAccessAsync(targetUserId, requesterId))
                    .Returns(Task.CompletedTask);

            _postRepo.Setup(r => r.GetPostsByUserIdAsync(targetUserId, 1, 20))
                     .ReturnsAsync(posts);

            _mapper.Setup(m => m.Map<List<PostDto>>(posts))
                   .Returns(new List<PostDto>
                   {
                       new PostDto { Id = 1, ImageUrl = "u1" },
                       new PostDto { Id = 2, ImageUrl = "u2" }
                   });

            // Act
            var result = await _sut.GetPostsAsync(targetUserId, requesterId);

            // Assert
            result.Should().HaveCount(2);
            _privacy.Verify(p => p.EnsureCanAccessAsync(targetUserId, requesterId), Times.Once);
            _postRepo.Verify(r => r.GetPostsByUserIdAsync(targetUserId, 1, 20), Times.Once);
            _mapper.Verify(m => m.Map<List<PostDto>>(posts), Times.Once);
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldThrow_WhenNotFound()
        {
            // Arrange
            var postId = 999;
            var userId = Guid.NewGuid();

            _postRepo.Setup(r => r.GetPostByIdAndUserAsync(postId, userId))
                     .ReturnsAsync((Post?)null);

            // Act
            Func<Task> act = async () => await _sut.GetPostByIdAsync(postId, userId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("Post not found.");

            _postRepo.Verify(r => r.GetPostByIdAndUserAsync(postId, userId), Times.Once);
        }

        [Fact]
        public async Task UpdatePostAsync_WhenImageProvided_ShouldDeleteOld_UploadNew_UpdateCaption_AndMap()
        {
            // Arrange
            var postId = 10;
            var userId = Guid.NewGuid();
            var existing = new Post
            {
                Id = postId,
                AuthorId = userId,
                ImageUrl = "https://old/image.jpg",
                Caption = "old"
            };

            var dto = new PostUpdateDto
            {
                Caption = "new caption",
                ImageFile = FakeFormFile("new.jpg")
            };

            _postRepo.Setup(r => r.GetPostByIdAndUserAsync(postId, userId))
                     .ReturnsAsync(existing);

            _fileStorage.Setup(s => s.DeleteFileAsync(existing.ImageUrl))
                        .Returns(Task.CompletedTask);
            _fileStorage.Setup(s => s.UploadFileAsync(dto.ImageFile!))
                        .ReturnsAsync("https://cdn/new.jpg");

            _postRepo.Setup(r => r.UpdateAsync(existing))
                     .Returns(Task.CompletedTask);

            var mapped = new PostDto { Id = postId, Caption = "new caption", ImageUrl = "https://cdn/new.jpg" };
            _mapper.Setup(m => m.Map<PostDto>(existing))
                   .Returns(mapped);

            // Act
            var result = await _sut.UpdatePostAsync(postId, dto, userId);

            // Assert
            result.ImageUrl.Should().Be("https://cdn/new.jpg");
            result.Caption.Should().Be("new caption");

            _fileStorage.Verify(s => s.DeleteFileAsync("https://old/image.jpg"), Times.Once);
            _fileStorage.Verify(s => s.UploadFileAsync(dto.ImageFile!), Times.Once);
            _postRepo.Verify(r => r.UpdateAsync(existing), Times.Once);
            _mapper.Verify(m => m.Map<PostDto>(existing), Times.Once);
        }

        [Fact]
        public async Task UpdatePostAsync_WhenNoImageAndOnlyCaption_ShouldUpdateCaption_AndMap()
        {
            // Arrange
            var postId = 11;
            var userId = Guid.NewGuid();
            var existing = new Post
            {
                Id = postId,
                AuthorId = userId,
                ImageUrl = "keep",
                Caption = "old"
            };

            var dto = new PostUpdateDto
            {
                Caption = "updated",
                ImageFile = null
            };

            _postRepo.Setup(r => r.GetPostByIdAndUserAsync(postId, userId))
                     .ReturnsAsync(existing);

            _postRepo.Setup(r => r.UpdateAsync(existing))
                     .Returns(Task.CompletedTask);

            _mapper.Setup(m => m.Map<PostDto>(existing))
                   .Returns(new PostDto { Id = postId, Caption = "updated", ImageUrl = "keep" });

            // Act
            var result = await _sut.UpdatePostAsync(postId, dto, userId);

            // Assert
            result.Caption.Should().Be("updated");
            result.ImageUrl.Should().Be("keep");

            _fileStorage.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
            _fileStorage.Verify(s => s.UploadFileAsync(It.IsAny<IFormFile>()), Times.Never);
            _postRepo.Verify(r => r.UpdateAsync(existing), Times.Once);
            _mapper.Verify(m => m.Map<PostDto>(existing), Times.Once);
        }

        [Fact]
        public async Task UpdatePostAsync_ShouldThrow_WhenPostNotAccessible()
        {
            // Arrange
            var postId = 12;
            var userId = Guid.NewGuid();
            var dto = new PostUpdateDto { Caption = "x" };

            _postRepo.Setup(r => r.GetPostByIdAndUserAsync(postId, userId))
                     .ReturnsAsync((Post?)null);

            // Act
            Func<Task> act = async () => await _sut.UpdatePostAsync(postId, dto, userId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("Post not found or not accessible.");

            _postRepo.Verify(r => r.GetPostByIdAndUserAsync(postId, userId), Times.Once);
        }

        [Fact]
        public async Task DeletePostAsync_ShouldReturnFalse_WhenPostNotFound()
        {
            // Arrange
            var postId = 200;
            var userId = Guid.NewGuid();

            _postRepo.Setup(r => r.GetPostByIdAndUserAsync(postId, userId))
                     .ReturnsAsync((Post?)null);

            // Act
            var ok = await _sut.DeletePostAsync(postId, userId);

            // Assert
            ok.Should().BeFalse();
            _postRepo.Verify(r => r.GetPostByIdAndUserAsync(postId, userId), Times.Once);
        }

        [Fact]
        public async Task DeletePostAsync_ShouldSoftDelete_Update_SaveChanges_AndReturnTrue()
        {
            // Arrange
            var postId = 201;
            var userId = Guid.NewGuid();
            var existing = new Post { Id = postId, AuthorId = userId, IsDeleted = false };

            _postRepo.Setup(r => r.GetPostByIdAndUserAsync(postId, userId))
                     .ReturnsAsync(existing);

            _postRepo.Setup(r => r.UpdateAsync(existing))
                     .Returns(Task.CompletedTask);
            _postRepo.Setup(r => r.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            // Act
            var ok = await _sut.DeletePostAsync(postId, userId);

            // Assert
            ok.Should().BeTrue();
            existing.IsDeleted.Should().BeTrue();
            existing.DeletedAt.Should().NotBeNull();

            _postRepo.Verify(r => r.UpdateAsync(existing), Times.Once);
            _postRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
*/
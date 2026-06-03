using AutoMapper;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Dtos.CommentDtos;
using SocialMediaPlatform.Entities.Dtos.FollowDtos;
using SocialMediaPlatform.Entities.Dtos.MediaDtos;
using SocialMediaPlatform.Entities.Dtos.MessageDtos;
using SocialMediaPlatform.Entities.Dtos.NotificationDtos;
using SocialMediaPlatform.Entities.Dtos.PostDtos;
using SocialMediaPlatform.Entities.Dtos.Story;
using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.Business.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<AppUser, UserDto>();
        CreateMap<AppUser, UserSummaryDto>();

        // Post
        CreateMap<Post, PostDto>()
            .ForMember(d => d.LikesCount, o => o.MapFrom(s => s.Likes.Count))
            .ForMember(d => d.CommentsCount, o => o.MapFrom(s => s.Comments.Count));

        // Comment
        CreateMap<Comment, CommentDto>()
            .ForMember(d => d.LikesCount, o => o.MapFrom(s => s.Likes.Count))
            .ForMember(d => d.RepliesCount, o => o.MapFrom(s => s.Replies.Count));

        // Story
        CreateMap<Story, StoryDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName))
            .ForMember(d => d.ProfilePictureUrl, o => o.MapFrom(s => s.User.ProfilePictureUrl))
            .ForMember(d => d.ViewsCount, o => o.MapFrom(s => s.Views.Count))
            .ForMember(d => d.LikesCount, o => o.MapFrom(s => s.Likes.Count));

        CreateMap<StoryView, StoryViewDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName))
            .ForMember(d => d.ProfilePictureUrl, o => o.MapFrom(s => s.User.ProfilePictureUrl));

        // Media
        CreateMap<Media, MediaDto>();

        // Message
        CreateMap<Message, MessageDto>();

        // Follow
        CreateMap<Follow, FollowDto>();

        // Likes
        CreateMap<PostLike, PostLikeDto>();
        CreateMap<CommentLike, CommentLikeDto>();
        CreateMap<StoryLike, StoryLikeDto>();

        // Notification
        CreateMap<Notification, NotificationDto>();
    }
}
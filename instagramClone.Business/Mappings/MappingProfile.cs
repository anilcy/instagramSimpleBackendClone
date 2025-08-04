using AutoMapper;
using instagramClone.Entities.Models;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Dtos.Story;

namespace instagramClone.Business.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Post entity'sini PostDto'ya dönüştürme kuralı
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author));

        // Eğer PostCreateDto veya PostUpdateDto dönüşümleri gerekiyorsa ekleyebilirsiniz:
        CreateMap<PostCreateDto, Post>();
        CreateMap<PostUpdateDto, Post>();

        // Comment mappings
        // Entity → DTO
        CreateMap<Comment, CommentDto>()
            // Temel alanlar (Id, PostId, Content, CreatedAt) AutoMapper otomatik halleder
            .ForMember(d => d.Author,
                opt => opt.MapFrom(s => s.Author))               // yazar bilgisini al
            .ForMember(d => d.ParentCommentId,
                opt => opt.MapFrom(s => s.ParentCommentId))     // reply-to id
            .ForMember(d => d.Replies,
                opt => opt.MapFrom(s => s.Replies))             // alt yorumlar
            .ForMember(d => d.RepliesCount,
                opt => opt.MapFrom(s => s.Replies.Count));     // alt yorum sayısı

        // DTO → Entity (yeni yorum eklerken)
        CreateMap<CreateCommentDto, Comment>()
            .ForMember(d => d.PostId,
                opt => opt.MapFrom(s => s.PostId))
            .ForMember(d => d.Content,
                opt => opt.MapFrom(s => s.Content))
            .ForMember(d => d.ParentCommentId,
                opt => opt.MapFrom(s => s.ParentCommentId));


        // Story mappings
        CreateMap<Story, StoryDto>()
            .ForMember(d => d.UserName,  o => o.MapFrom(s => s.User.UserName))
            .ForMember(d => d.ProfilePictureUrl, o => o.MapFrom(s => s.User.ProfilePictureUrl));

        CreateMap<StoryCreateDto, Story>();


        // User mappings
        CreateMap<AppUser, UserDto>();
        CreateMap<AppUser, UserSummaryDto>();
        
        // Message mappings
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender))
            .ForMember(dest => dest.Receiver, opt => opt.MapFrom(src => src.Receiver));
        
        CreateMap<CreateMessageDto, Message>();
        
        // Follow mappings
        CreateMap<Follow, FollowDto>()
            .ForMember(dest => dest.Follower, opt => opt.MapFrom(src => src.Follower))
            .ForMember(dest => dest.Followed, opt => opt.MapFrom(src => src.Followed));
    }
}

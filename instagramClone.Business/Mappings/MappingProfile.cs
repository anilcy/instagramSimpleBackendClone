using AutoMapper;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;

namespace instagramClone.Business.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Post entity'sini PostDto'ya dönüştürme kuralı
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));


        // Eğer PostCreateDto veya PostUpdateDto dönüşümleri gerekiyorsa ekleyebilirsiniz:
        CreateMap<PostCreateDto, Post>();
        CreateMap<PostUpdateDto, Post>();
    }
}    
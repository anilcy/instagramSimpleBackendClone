using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;

namespace instagramClone.Business.Services
{
    public class PrivacyService : IPrivacyService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFollowRepository _followRepository;

        public PrivacyService(IUserRepository userRepository, IFollowRepository followRepository)
        {
            _userRepository = userRepository;
            _followRepository = followRepository;
        }
        
        public async Task EnsureCanAccessAsync(Guid targetUserId, Guid? requesterId)
        {
            var user = await _userRepository.GetByIdAsync(targetUserId) ?? throw new ArgumentException("User not found");
            if (!user.IsPrivate)
                return;

            if (!requesterId.HasValue || requesterId.Value != targetUserId)
            {
                var isFollowing = requesterId.HasValue && await _followRepository.IsFollowingAsync(requesterId.Value, targetUserId);
                if (!isFollowing)
                    throw new UnauthorizedAccessException("User's content is private");
            }
        }
    }
}
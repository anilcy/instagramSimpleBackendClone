
namespace instagramClone.Business.Interfaces
{
    public interface IPrivacyService
    {
        Task EnsureCanAccessAsync(Guid targetUserId, Guid? requesterId);
    }
}
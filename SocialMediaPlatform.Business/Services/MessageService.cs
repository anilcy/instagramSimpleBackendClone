using AutoMapper;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Business.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessageService(IMessageRepository messageRepository, IMapper mapper)
    {
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    public async Task<MessageDto> SendMessageAsync(Guid senderId, CreateMessageDto dto)
    {
        var msg = new Message
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await _messageRepository.InsertAsync(msg);
        await _messageRepository.SaveChangesAsync();

        return _mapper.Map<MessageDto>(msg);
    }

    public async Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50)
    {
        var msgs = await _messageRepository.GetConversationAsync(userId, otherUserId, page, pageSize);
        return _mapper.Map<List<MessageDto>>(msgs);
    }

    public async Task<MessageDto?> GetByIdAsync(int id)
    {
        var msg = await _messageRepository.GetByIdAsync(id);
        return _mapper.Map<MessageDto?>(msg);
    }

    public async Task<bool> MarkAsReadAsync(int messageId, Guid readerId)
    {
        var msg = await _messageRepository.GetByIdAsync(messageId);
        if (msg == null || msg.ReceiverId != readerId) return false;

        msg.IsRead = true;
        msg.ReadAt = DateTime.UtcNow;

        await _messageRepository.UpdateAsync(msg);
        await _messageRepository.SaveChangesAsync();

        return true;
    }

    public Task<int> GetUnreadCountAsync(Guid userId, Guid fromUserId)
        => _messageRepository.GetUnreadMessagesCountAsync(userId, fromUserId);

    public async Task MarkMessagesAsReadAsync(Guid userId, Guid fromUserId)
    {
        await _messageRepository.MarkMessagesAsReadAsync(userId, fromUserId);
    }
}
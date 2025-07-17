using AutoMapper;
using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;

namespace instagramClone.Business.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessageService(IMessageRepository messageRepository, IMapper mapper)
    {
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    public async Task<MessageDto> SendMessageAsync(Guid senderId, CreateMessageDto messageDto)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = messageDto.ReceiverId,
            Content = messageDto.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepository.InsertAsync(message);
        await _messageRepository.SaveChangesAsync();
        return _mapper.Map<MessageDto>(message);
    }

    public async Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50)
    {
        var messages = await _messageRepository.GetConversationAsync(userId, otherUserId, page, pageSize);
        return _mapper.Map<List<MessageDto>>(messages);
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
    {
        var messages = await _messageRepository.GetConversationsAsync(userId);
        var conversations = new List<ConversationDto>();

        var groupedMessages = messages.GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId);

        foreach (var group in groupedMessages)
        {
            var otherUserId = group.Key;
            var lastMessage = group.OrderByDescending(m => m.CreatedAt).First();
            var unreadCount = await _messageRepository.GetUnreadMessagesCountAsync(userId, otherUserId);

            var conversation = new ConversationDto
            {
                OtherUser = _mapper.Map<UserSummaryDto>(lastMessage.SenderId == userId ? lastMessage.Receiver : lastMessage.Sender),
                LastMessage = _mapper.Map<MessageDto>(lastMessage),
                UnreadCount = unreadCount
            };

            conversations.Add(conversation);
        }

        return conversations.OrderByDescending(c => c.LastMessage?.CreatedAt).ToList();
    }

    public async Task MarkMessagesAsReadAsync(Guid userId, Guid fromUserId)
    {
        await _messageRepository.MarkMessagesAsReadAsync(userId, fromUserId);
    }

    public async Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid fromUserId)
    {
        return await _messageRepository.GetUnreadMessagesCountAsync(userId, fromUserId);
    }
}
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
	public class MessageRepository : IMessagesRepository
	{
		private readonly DataContext _context;
        private readonly IMapper _mapper;

		public MessageRepository(DataContext context, IMapper mapper)
		{
            _mapper = mapper;
			_context = context;
		}

		public void AddGroup(Group group)
		{
			_context.Groups.Add(group);
		}

		public void AddMessage(Message message)
		{
			_context.Messages.Add(message);
		}

		public void DeleteMessageThread(Message message)
		{
			_context.Messages.Remove(message);
		}

		public async Task<Connection> GetConnection(string connectionId)
		{
			return await _context.Connections.FindAsync(connectionId);
		}

		public async Task<Group> GetGroupForConnection(string connectionId)
		{
			return await _context.Groups.Include(group => group.Connections)
			.Where(group => group.Connections.Any(connection => connection.ConnectionId == connectionId))
			.FirstOrDefaultAsync();
		}

		public async Task<Message> GetMessage(int id)
		{
			return await _context.Messages.FindAsync(id);
		}

		public async Task<Group> GetMessageGroup(string groupName)
		{
			return await _context.Groups.Include(group => group.Connections).FirstOrDefaultAsync(group => group.Name == groupName);
		}

		public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
		{
			var query = _context.Messages.OrderByDescending(message => message.MessageSent)
			.AsQueryable();

			query = messageParams.Container switch
			{
				"Inbox" => query.Where(message => message.RecipientUsername == messageParams.Username 
				&& message.RecipientDeleted == false),
				"Outbox" => query.Where(message => message.SenderUsername == messageParams.Username
				&& message.SenderDeleted == false),
				_ =>  query.Where(message => message.RecipientUsername == messageParams.Username 
				&& message.RecipientDeleted == false
				&& message.DateRead == null)
			};

			var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
			
			return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);

		}

		public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
		{
			var messages = await _context.Messages
			.Include(message => message.Sender).ThenInclude(user => user.Photos)
			.Where(
				message => message.RecipientUsername == currentUserName
				&& message.RecipientDeleted == false
				&& message.SenderUsername == recipientUserName
				|| message.RecipientUsername == recipientUserName 
				&& message.SenderDeleted == false
				&& message.SenderUsername == currentUserName
			)
			.OrderBy(message => message.MessageSent)
			.ToListAsync();

			var unreadMessages = messages.Where(message => message.DateRead == null
			&& message.RecipientUsername == currentUserName).ToList();

			if (unreadMessages.Any())
			{
				foreach (var message in unreadMessages)
				{
					message.DateRead = DateTime.UtcNow;
				}

				 await _context.SaveChangesAsync();
			}

			return _mapper.Map<IEnumerable<MessageDto>>(messages);
		}

		public void RemoveConnection(Connection connection)
		{
			_context.Connections.Remove(connection);
		}

		public async Task<bool> SaveAllAsync()
		{
			return await _context.SaveChangesAsync() > 0;
		}
	}
}
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

		public void AddMessage(Message message)
		{
			_context.Messages.Add(message);
		}

		public void DeleteMessage(Message message)
		{
			_context.Messages.Remove(message);
		}

		public async Task<Message> GetMessage(int id)
		{
			return await _context.Messages.FindAsync(id);
		}

		public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
		{
			var query = _context.Messages.OrderByDescending(message => message.MessageSent)
			.AsQueryable();

			query = messageParams.Container switch
			{
				"Inbox" => query.Where(user => user.RecipientUsername == messageParams.Username),
				"Outbox" => query.Where(user => user.SenderUsername == messageParams.Username),
				_ =>  query.Where(user => user.RecipientUsername == messageParams.Username && user.DateRead == null)
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
				&& message.SenderUsername == recipientUserName
				|| message.RecipientUsername == recipientUserName 
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

		public async Task<bool> SaveAllAsync()
		{
			return await _context.SaveChangesAsync() > 0;
		}
	}
}
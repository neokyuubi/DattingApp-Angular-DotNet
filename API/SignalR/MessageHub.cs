using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
	[Authorize]
	public class MessageHub : Hub
	{
		private readonly IMessagesRepository _messagesRepository;
		private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;

		public MessageHub(IMessagesRepository messagesRepository, IUserRepository userRepository, IMapper mapper)
		{
			_userRepository = userRepository;
			_mapper = mapper;
			_messagesRepository = messagesRepository;
		}

		public override async Task OnConnectedAsync()
		{
			var httpContext = Context.GetHttpContext();
			var otherUser = httpContext.Request.Query["user"];
			var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
			await AddToGroup(groupName);
			var messages = await _messagesRepository.GetMessageThread(Context.User.GetUsername(), otherUser);
			await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
		}
		
		public override async Task OnDisconnectedAsync(Exception exception)
		{
			await RemoveFromMessageGroup();
			await base.OnDisconnectedAsync(exception);
		}

		public async Task SendMessage(CreateMessageDto createMessageDto)
		{
			var username = Context.User.GetUsername();
			if (username == createMessageDto.RecipientUsername.ToLower())
			{
				throw new HubException("You cannot send message to yourself");
			}

			var sender = await _userRepository.GetUserByUsernameAsync(username);
			var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername) ?? throw new HubException("Not Found");

			var message = new Message
			{
				Sender = sender,
				Recipient = recipient,
				SenderUsername = sender.UserName,
				RecipientUsername = recipient.UserName,
				Content = createMessageDto.Content
			};

			var groupName = GetGroupName(sender.UserName, recipient.UserName);

			var group = await _messagesRepository.GetMessageGroup(groupName);

			if (group.Connections.Any(connection => connection.Username == recipient.UserName))
			{
				message.DateRead = DateTime.UtcNow;
			}

			_messagesRepository.AddMessage(message);

			if (await _messagesRepository.SaveAllAsync())
			{
				await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
			}
		}

		private static string GetGroupName(string caller, string other)
		{
			var stringCompare = string.CompareOrdinal(caller, other) < 0;
			return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
		}

		private async Task<bool> AddToGroup(string groupName)
		{
			var group = await _messagesRepository.GetMessageGroup(groupName);
			var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
			if (group == null)
			{
				group = new Group(groupName);
				_messagesRepository.AddGroup(group);
			}

			group.Connections.Add(connection);

			return await _messagesRepository.SaveAllAsync();
		}

		private async Task RemoveFromMessageGroup()
		{
			var connection = await _messagesRepository.GetConnection(Context.ConnectionId);
			_messagesRepository.RemoveConnection(connection);
			await _messagesRepository.SaveAllAsync();
		}
	}
}
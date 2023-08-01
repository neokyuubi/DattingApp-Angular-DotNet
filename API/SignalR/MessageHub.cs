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
        private readonly IHubContext<PresenceHub> _presenceHub;

		public MessageHub(IMessagesRepository messagesRepository, IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> presenceHub)
		{
            _presenceHub = presenceHub;
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
			var group = await AddToGroup(groupName);

			await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

			var messages = await _messagesRepository.GetMessageThread(Context.User.GetUsername(), otherUser);
			await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
		}
		
		public override async Task OnDisconnectedAsync(Exception exception)
		{
		 	var group = await RemoveFromMessageGroup();
			await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
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
			else
			{
				var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
				if (connections != null)
				{
					await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new {username = sender.UserName, knownAs = sender.KnownAs});
				}
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

		private async Task<Group> AddToGroup(string groupName)
		{
			var group = await _messagesRepository.GetMessageGroup(groupName);
			var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
			if (group == null)
			{
				group = new Group(groupName);
				_messagesRepository.AddGroup(group);
			}

			group.Connections.Add(connection);

			if(await _messagesRepository.SaveAllAsync()) return group;
			throw new HubException("Failed to add to group");
		}

		private async Task<Group> RemoveFromMessageGroup()
		{
			var group = await _messagesRepository.GetGroupForConnection(Context.ConnectionId);
			var connection = group.Connections.FirstOrDefault(cnt => cnt.ConnectionId == Context.ConnectionId);
			_messagesRepository.RemoveConnection(connection);
			if(await _messagesRepository.SaveAllAsync()) return group;
			throw new HubException("Failed to remove from group");
		}
	}
}
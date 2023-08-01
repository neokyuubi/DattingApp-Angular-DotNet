using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
	public class MessageHub : Hub
    {
		private readonly IMessagesRepository _messagesRepository;

		public MessageHub(IMessagesRepository messagesRepository)
		{
			_messagesRepository = messagesRepository;
		}

		public override async Task OnConnectedAsync()
		{
			var httpContext = Context.GetHttpContext();
			var otherUser = httpContext.Request.Query["user"];
			var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
			var messages = await _messagesRepository.GetMessageThread(Context.User.GetUsername(), otherUser);
			await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
		}

		private static string GetGroupName(string caller, string other)
		{
			var stringCompare = string.CompareOrdinal(caller, other) < 0;
			return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			return base.OnDisconnectedAsync(exception);
		}
	}
}
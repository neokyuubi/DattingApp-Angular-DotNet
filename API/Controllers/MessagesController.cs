using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	public class MessagesController: BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork uw;
        public MessagesController(IMapper mapper, IUnitOfWork uw)
		{
            this.uw = uw;
            _mapper = mapper;
		}

		[HttpPost]
		public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
		{
			var username = User.GetUsername();
			if (username == createMessageDto.RecipientUsername.ToLower())
			{
				return BadRequest("You cannot send message to yourself");
			}
			var sender = await uw.UserRepository.GetUserByUsernameAsync(username);
			var recipient = await uw.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

			if (recipient == null) return NotFound();

			var message = new Message
			{
				Sender = sender,
				Recipient = recipient,
				SenderUsername = sender.UserName,
				RecipientUsername = recipient.UserName,
				Content = createMessageDto.Content
			};

			uw.MessagesRepository.AddMessage(message);

			if(await uw.Complete()) return Ok(_mapper.Map<MessageDto>(message));

			return BadRequest("Failed to send message");
		}

		[HttpGet]
		public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
		{
			messageParams.Username = User.GetUsername();
			var messages =  await uw.MessagesRepository.GetMessagesForUser(messageParams);

			Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));
			return messages;
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteMessageThread(int id)
		{
			var username = User.GetUsername();
			var message = await uw.MessagesRepository.GetMessage(id);

			if (message.SenderUsername  != username && message.RecipientUsername != username) return Unauthorized();

			if (message.SenderUsername == username) message.SenderDeleted = true;
			if (message.RecipientUsername == username) message.RecipientDeleted = true;

			if(message.SenderDeleted && message.RecipientDeleted)
			{
				uw.MessagesRepository.DeleteMessageThread(message);	
			}

			if(await uw.Complete()) return Ok();
		
			return BadRequest("Problem deleting the message");
		}
    }
}
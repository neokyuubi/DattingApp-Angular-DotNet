using System.Collections;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
	public interface IMessagesRepository
    {
        void AddMessage(Message message);
		
		void DeleteMessageThread(Message message);

		Task<Message> GetMessage(int id);

		Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);

		Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName);

		Task<bool> SaveAllAsync();
    }
}
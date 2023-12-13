using Harjoitus.Models;
using Harjoitus.Repositories;
using Humanizer;

namespace Harjoitus.Services
{
    public class MessageService : IMessageService
    {
        
        private readonly IMessageRepository _repository;
        private readonly IUserRepository _userRepository;
        
        public MessageService(IMessageRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;

        }

        public async Task<bool> DeleteMessageAsync(long id)
        {
            Message? message = await _repository.GetMessageAsync(id);
            if(message != null)
            {
                await _repository.DeleteMessageAsync(message);
                return true;

            }
            return false;
        }

        public async Task<MessageDTO?> GetMessageAsync(long id)
        {
            return MessageToDTO(await _repository.GetMessageAsync(id));

        }


        public async Task<MessageDTO> NewMessageAsync(MessageDTO message)
        {
            // TODO: check if receiver is real

            return MessageToDTO(await _repository.NewMessageAsync(await DTOToMessage(message)));
            
        }

        public async Task<bool> UpdateMessageAsync(MessageDTO message)
        {
            Message? updatedMessage = await _repository.GetMessageAsync(message.Id);
            if (updatedMessage != null)
            {
                return await _repository.UpdateMessageAsync(await DTOToMessage(message));          
            }
            return false;
        }
        
        public async Task<IEnumerable<MessageDTO>> GetMessagesAsync()
        {

            IEnumerable<Message> messages = await _repository.GetMessagesAsync();
            List<MessageDTO> result = new List<MessageDTO>();

            foreach(Message message in messages)
            {
                result.Add(MessageToDTO(message));
            }

            return result;

        }

        public async Task<IEnumerable<MessageDTO>> SearchMessagesAsync(string searchtext)
        {
            IEnumerable<Message> messages = await _repository.SearchMessagesAsync(searchtext);
            List<MessageDTO> result = new List<MessageDTO>();
            foreach(Message message in messages)
            {
                result.Add(MessageToDTO(message));
            }
            return result;
        }


        public async Task<IEnumerable<MessageDTO>?> GetSentMessagesAsync(string username)
        {
            User? user = await _userRepository.GetUserAsync(username);
            if(user == null)
            {
                return null;
            }

            IEnumerable<Message> messages = await _repository.GetSentMessagesAsync(user);
            List<MessageDTO> result = new List<MessageDTO>();

            foreach (Message message in messages)
            {
                result.Add(MessageToDTO(message));
            }

            return result;
        }

        public async Task<IEnumerable<MessageDTO>?> GetReceivedMessagesAsync(string username)
        {
            User? user = await _userRepository.GetUserAsync(username);
            if (user == null)
            {
                return null;
            }

            IEnumerable<Message> messages = await _repository.GetReceivedMessagesAsync(user);
            List<MessageDTO> result = new List<MessageDTO>();

            foreach (Message message in messages)
            {
                result.Add(MessageToDTO(message));
            }

            return result;
        }

        private MessageDTO MessageToDTO(Message message)
        {
            MessageDTO messageDTO = new MessageDTO();
            messageDTO.Id = message.Id;
            messageDTO.Title = message.Title;
            messageDTO.Body = message.Body;
            messageDTO.Sender = message.Sender.UserName;
            if(message.Recipient != null)
            {
                messageDTO.Recipient = message.Recipient.UserName;
            }
            if(message.PrevMessage != null)
            {
                messageDTO.PrevMessageId = message.PrevMessage.Id;
            }

            return messageDTO;
        }

        private async Task<Message?> DTOToMessage(MessageDTO dto)
        {
            Message newMessage = new Message();
            newMessage.Id = dto.Id;
            newMessage.Title = dto.Title;
            newMessage.Body = dto.Body;

            User? sender = await _userRepository.GetUserAsync(dto.Sender);
            if(sender != null)
            {
                newMessage.Sender = sender;
            }
            if (dto.Recipient != null)
            {
                User? recipient = await _userRepository.GetUserAsync(dto.Recipient);
                if(recipient == null)
                {
                    return null;
                }
                newMessage.Recipient = recipient;
            }
            if(dto.PrevMessageId !=null && dto.PrevMessageId>0)
            {
                Message prevMessage = await _repository.GetMessageAsync((long)dto.PrevMessageId);
                if(prevMessage == null)
                {
                    return null;
                }
                newMessage.PrevMessage = prevMessage;
            }
            return newMessage;
        }


    }
}

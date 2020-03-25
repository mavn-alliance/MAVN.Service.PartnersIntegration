using System.Threading.Tasks;
using Lykke.Service.PartnersIntegration.Domain.Models;

namespace Lykke.Service.PartnersIntegration.Domain.Services
{
    public interface IMessagesService
    {
        Task<MessagesPostResponse> SendMessageAsync(MessagesPostRequest contract);

        Task<MessageGetResponse> GetMessageAsync(string partnerMessageId);

        Task DeleteMessageAsync(string partnerMessageId);
    }
}

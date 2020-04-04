using System.Threading.Tasks;
using MAVN.Service.PartnersIntegration.Domain.Models;

namespace MAVN.Service.PartnersIntegration.Domain.Services
{
    public interface IMessagesService
    {
        Task<MessagesPostResponse> SendMessageAsync(MessagesPostRequest contract);

        Task<MessageGetResponse> GetMessageAsync(string partnerMessageId);

        Task DeleteMessageAsync(string partnerMessageId);
    }
}

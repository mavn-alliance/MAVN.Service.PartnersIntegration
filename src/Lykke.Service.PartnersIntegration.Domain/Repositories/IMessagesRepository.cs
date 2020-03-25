using System;
using System.Threading.Tasks;
using Lykke.Service.PartnersIntegration.Domain.Models;

namespace Lykke.Service.PartnersIntegration.Domain.Repositories
{
    public interface IMessagesRepository
    {
        Task<Guid> InsertAsync(MessagesPostRequest contract);

        Task<MessageGetResponse> GetByIdAsync(string partnerMessageId);

        Task DeleteMessageAsync(string partnerMessageId);
    }
}

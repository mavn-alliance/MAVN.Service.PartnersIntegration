using System;
using System.Threading.Tasks;
using MAVN.Service.PartnersIntegration.Domain.Models;

namespace MAVN.Service.PartnersIntegration.Domain.Repositories
{
    public interface IMessagesRepository
    {
        Task<Guid> InsertAsync(MessagesPostRequest contract);

        Task<MessageGetResponse> GetByIdAsync(string partnerMessageId);

        Task DeleteMessageAsync(string partnerMessageId);
    }
}

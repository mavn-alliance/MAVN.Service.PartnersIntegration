using System.Threading.Tasks;
using MAVN.Service.PartnersIntegration.Client.Models;
using Refit;

namespace MAVN.Service.PartnersIntegration.Client.Api
{
    /// <summary>
    /// PartnersIntegration Messages client API interface.
    /// </summary>
    public interface IMessagesApi
    {
        /// <summary>
        /// Endpoint providing a way to send a message from a partner to a customer
        /// </summary>
        /// <param name="model">Contains information on the message to be sent</param>
        /// <returns>Message sent info</returns>
        [Post("/api/messages")]
        Task<MessagesPostResponseModel> SendMessageAsync(MessagesPostRequestModel model);

        /// <summary>
        /// Endpoint providing a way to retrieve message information
        /// </summary>
        /// <param name="partnerMessageId">The id of the message</param>
        /// <returns>Message info</returns>
        [Get("/api/messages/{partnerMessageId}")]
        Task<MessageGetResponseModel> GetMessageAsync(string partnerMessageId);

        /// <summary>
        /// Endpoint providing a way to delete message
        /// </summary>
        /// <param name="partnerMessageId">The id of the message</param>
        /// <returns></returns>
        [Delete("/api/messages/{partnerMessageId}")]
        Task DeleteMessageAsync(string partnerMessageId);
    }
}

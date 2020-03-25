using Lykke.Service.PartnersIntegration.Client.Enums;

namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Response model for sending a message from a partner to a customer
    /// </summary>
    public class MessagesPostResponseModel
    {
        /// <summary>
        /// The message id
        /// </summary>
        public string PartnerMessageId { get; set; }

        /// <summary>
        /// Error code if any
        /// </summary>
        public MessagesErrorCode ErrorCode { get; set; }
    }
}

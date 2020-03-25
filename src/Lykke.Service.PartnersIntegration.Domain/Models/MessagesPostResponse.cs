using Lykke.Service.PartnersIntegration.Domain.Enums;

namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class MessagesPostResponse
    {
        public string PartnerMessageId { get; set; }

        public MessagesErrorCode ErrorCode { get; set; }
    }
}

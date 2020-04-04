using MAVN.Service.PartnersIntegration.Domain.Enums;

namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class MessagesPostResponse
    {
        public string PartnerMessageId { get; set; }

        public MessagesErrorCode ErrorCode { get; set; }
    }
}

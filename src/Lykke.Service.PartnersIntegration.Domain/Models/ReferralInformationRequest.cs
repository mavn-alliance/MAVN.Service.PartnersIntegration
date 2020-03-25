namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class ReferralInformationRequest
    {
        public string CustomerId { get; set; }

        public string PartnerId { get; set; }

        public string ExternalLocationId { get; set; }
    }
}

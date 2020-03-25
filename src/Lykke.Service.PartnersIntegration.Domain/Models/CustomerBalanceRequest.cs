namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class CustomerBalanceRequest
    {
        public string PartnerId { get; set; }

        public string ExternalLocationId { get; set; }
        
        public string Currency { get; set; }
    }
}

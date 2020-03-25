using Lykke.Service.PartnersIntegration.Domain.Enums;

namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class CustomerInformationResponse
    {
        public string CustomerId { get; set; }

        public CustomerInformationStatus Status { get; set; }

        public string TierLevel { get; set; }
        
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}

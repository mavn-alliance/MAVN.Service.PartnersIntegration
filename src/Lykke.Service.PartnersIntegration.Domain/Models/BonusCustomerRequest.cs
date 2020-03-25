using System;

namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class BonusCustomerRequest
    {
        public string CustomerId { get; set; }

        public string Email { get; set; }

        public decimal? FiatAmount { get; set; }

        public string Currency { get; set; }
        
        public DateTime? PaymentTimestamp { get; set; }

        public string PartnerId { get; set; }

        public string ExternalLocationId { get; set; }

        public string PosId { get; set; }
    }
}

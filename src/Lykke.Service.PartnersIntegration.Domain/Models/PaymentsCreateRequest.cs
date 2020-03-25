using Falcon.Numerics;

namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class PaymentsCreateRequest
    {
        public string CustomerId { get; set; }

        public decimal? TotalFiatAmount { get; set; }

        public decimal? FiatAmount { get; set; }

        public string Currency { get; set; }

        public Money18? TokensAmount { get; set; }

        public string PaymentInfo { get; set; }

        public string PartnerId { get; set; }

        public string ExternalLocationId { get; set; }

        public string PosId { get; set; }

        public string PaymentProcessedCallbackUrl { get; set; }
        
        public string RequestAuthToken { get; set; }

        public int? ExpirationTimeoutInSeconds { get; set; }
    }
}

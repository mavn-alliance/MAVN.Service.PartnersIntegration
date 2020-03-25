using Lykke.Service.PartnersIntegration.Domain.Enums;

namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class PaymentsCreateResponse
    {
        public PaymentCreateStatus Status { get; set; }

        public string PaymentRequestId { get; set; }
    }
}

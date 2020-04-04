using MAVN.Service.PartnersIntegration.Domain.Enums;

namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class PaymentsCreateResponse
    {
        public PaymentCreateStatus Status { get; set; }

        public string PaymentRequestId { get; set; }
    }
}

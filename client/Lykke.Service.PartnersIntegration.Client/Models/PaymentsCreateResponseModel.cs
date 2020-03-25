using Lykke.Service.PartnersIntegration.Client.Enums;

namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Response of creating  a payment request to reserve tokens
    /// </summary>
    public class PaymentsCreateResponseModel
    {
        /// <summary>
        /// Status of the request
        /// </summary>
        public PaymentCreateStatus Status { get; set; }

        /// <summary>
        /// The Id of the payment request
        /// </summary>
        public string PaymentRequestId { get; set; }
    }
}

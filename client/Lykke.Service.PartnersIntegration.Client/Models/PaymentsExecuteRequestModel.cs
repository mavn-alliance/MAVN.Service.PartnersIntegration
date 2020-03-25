using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Model used for executing a payment request
    /// </summary>
    public class PaymentsExecuteRequestModel
    {
        /// <summary>
        /// The payment request id
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// The partner id
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string PartnerId { get; set; }
    }
}

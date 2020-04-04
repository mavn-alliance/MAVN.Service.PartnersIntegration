using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAVN.Service.PartnersIntegration.MsSqlRepositories.Entities
{
    [Table("payment_processed_callback_url")]
    public class PaymentProcessedCallbackUrlEntity : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string PaymentRequestId { get; set; }

        [Required]
        [MaxLength(512)]
        public string Url { get; set; }

        [Required]
        [MaxLength(100)]
        public string RequestAuthToken { get; set; }
    }
}

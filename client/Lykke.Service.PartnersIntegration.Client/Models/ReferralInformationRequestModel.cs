using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Model containing information about the request for balance info
    /// </summary>
    public class ReferralInformationRequestModel
    {
        /// <summary>
        /// Customer id
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CustomerId { get; set; }

        /// <summary>
        /// The partner's Id
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string PartnerId { get; set; }

        /// <summary>
        /// Hotel/location ID
        /// </summary>
        [MaxLength(100)]
        public string ExternalLocationId { get; set; }
    }
}

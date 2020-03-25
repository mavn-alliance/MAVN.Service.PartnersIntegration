using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Model used to send a message from a partner to a customer
    /// </summary>
    public class MessagesPostRequestModel
    {
        /// <summary>
        /// The partner's id 
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string PartnerId { get; set; }

        /// <summary>
        /// The customer's id
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CustomerId { get; set; }

        /// <summary>
        /// The subject of the message
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }

        /// <summary>
        /// The message
        /// </summary>
        [Required]
        [MaxLength(5120)]
        public string Message { get; set; }

        /// <summary>
        /// The partner's location id
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ExternalLocationId { get; set; }

        /// <summary>
        /// The pos id
        /// </summary>
        [MaxLength(100)]
        public string PosId { get; set; }

        /// <summary>
        /// If there should be a push notification sent
        /// </summary>
        [Required]
        public bool? SendPushNotification { get; set; }
    }
}

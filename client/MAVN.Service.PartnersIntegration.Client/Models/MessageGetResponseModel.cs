using System;

namespace MAVN.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Contains information about a message
    /// </summary>
    public class MessageGetResponseModel
    {
        /// <summary>
        /// When the message was created
        /// </summary>
        public DateTime CreationTimestamp { get; set; }

        /// <summary>
        /// The partner's id 
        /// </summary>
        public string PartnerId { get; set; }

        /// <summary>
        /// The customer's id
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// The subject of the message
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The partner's external location id
        /// </summary>
        public string ExternalLocationId { get; set; }

        /// <summary>
        /// The pos id
        /// </summary>
        public string PosId { get; set; }
    }
}

using System;

namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class MessageGetResponse
    {
        public DateTime CreationTimestamp { get; set; }

        public string PartnerId { get; set; }

        public string CustomerId { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public string ExternalLocationId { get; set; }

        public string PosId { get; set; }
    }
}

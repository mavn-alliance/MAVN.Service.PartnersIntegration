namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class MessagesPostRequest
    {
        public string PartnerId { get; set; }

        public string CustomerId { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public string ExternalLocationId { get; set; }

        public string PosId { get; set; }

        public bool? SendPushNotification { get; set; }
    }
}

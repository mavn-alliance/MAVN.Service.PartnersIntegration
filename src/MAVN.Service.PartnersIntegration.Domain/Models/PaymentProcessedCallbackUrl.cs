namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class PaymentProcessedCallbackUrl
    {
        public string PaymentRequestId { get; set; }

        public string Url { get; set; }

        public string RequestAuthToken { get; set; }
    }
}

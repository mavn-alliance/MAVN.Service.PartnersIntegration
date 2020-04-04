using Falcon.Numerics;

namespace MAVN.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Used to create a payment request to reserve tokens
    /// </summary>
    public class PaymentsCreateRequestModel
    {
        /// <summary>
        /// The Customer's Id
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// The total payment amount in fiat 
        /// </summary>
        public decimal? TotalFiatAmount { get; set; }

        /// <summary>
        /// Amount to reserve in fiat (Required if no TokensAmount)
        /// </summary>
        public decimal? FiatAmount { get; set; }

        /// <summary>
        /// Fiat currency (Required if Fiat is used)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Amount to reserve in tokens (Required if no FiatAmount)
        /// </summary>
        public Money18? TokensAmount { get; set; }

        /// <summary>
        /// Additional information about the payment. Will be shown to the customer
        /// </summary>
        public string PaymentInfo { get; set; }

        /// <summary>
        /// The id of the partner sending the request
        /// </summary>
        public string PartnerId { get; set; }

        /// <summary> 
        /// The location id of the partner sending the request
        /// </summary>
        public string ExternalLocationId { get; set; }

        /// <summary>
        /// The pos id 
        /// </summary>
        public string PosId { get; set; }

        /// <summary>
        /// Url used to send a callback when processing finished
        /// </summary>
        public string PaymentProcessedCallbackUrl { get; set; }

        /// <summary>
        /// AuthToken to be used with Callback Url
        /// </summary>
        public string RequestAuthToken { get; set; }

        /// <summary>
        /// Expiration timeout in seconds
        /// </summary>
        public int? ExpirationTimeoutInSeconds { get; set; }
    }
}

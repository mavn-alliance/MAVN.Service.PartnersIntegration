using System;
using Falcon.Numerics;
using MAVN.Service.PartnersIntegration.Client.Enums;

namespace MAVN.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Information about the payment request status
    /// </summary>
    public class PaymentRequestStatusResponseModel
    {
        /// <summary>
        /// The status of the payment request
        /// </summary>
        public PaymentRequestStatus Status { get; set; }

        /// <summary>
        /// The total fiat amount
        /// </summary>
        public decimal TotalFiatAmount { get; set; }

        /// <summary>
        /// The fiat amount
        /// </summary>
        public decimal FiatAmount { get; set; }

        /// <summary>
        /// The fiat currency
        /// </summary>
        public string FiatCurrency { get; set; }

        /// <summary>
        /// MVN Tokens
        /// </summary>
        public Money18 TokensAmount { get; set; }

        /// <summary>
        /// The payment request timestamp
        /// </summary>
        public DateTime PaymentRequestTimestamp { get; set; }

        /// <summary>
        /// The payment request timestamp
        /// </summary>
        public DateTime PaymentRequestCustomerExpirationTimestamp { get; set; }

        /// <summary>
        /// Timestamp of the tokens reservation
        /// </summary>
        public DateTime? PaymentRequestApprovedTimestamp { get; set; }

        /// <summary>
        /// Timestamp when tokens were burn
        /// </summary>
        public DateTime? PaymentExecutionTimestamp { get; set; }

    }
}

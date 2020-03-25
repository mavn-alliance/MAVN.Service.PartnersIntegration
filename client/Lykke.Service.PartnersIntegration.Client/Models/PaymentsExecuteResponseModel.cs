using System;
using Falcon.Numerics;
using Lykke.Service.PartnersIntegration.Client.Enums;

namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Response of payment request execution
    /// </summary>
    public class PaymentsExecuteResponseModel
    {
        /// <summary>
        /// The status of the execution
        /// </summary>
        public PaymentExecuteStatus Status { get; set; }

        /// <summary>
        /// The payment id
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// The customer id
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// MVN Tokens
        /// </summary>
        public Money18 TokensAmount { get; set; }

        /// <summary>
        /// The fiat amount
        /// </summary>
        public decimal FiatAmount { get; set; }

        /// <summary>
        /// The fiat currency 
        /// </summary>
        public string Currency { get; set; }
    }
}

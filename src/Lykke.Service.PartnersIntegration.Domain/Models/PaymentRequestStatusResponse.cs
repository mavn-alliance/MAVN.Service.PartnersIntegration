using System;
using Falcon.Numerics;
using Lykke.Service.PartnersIntegration.Domain.Enums;

namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class PaymentRequestStatusResponse
    {
        public PaymentRequestStatus Status { get; set; }

        public decimal TotalFiatAmount { get; set; }

        public decimal FiatAmount { get; set; }

        public string FiatCurrency { get; set; }

        public Money18 TokensAmount { get; set; }

        public DateTime PaymentRequestTimestamp { get; set; }

        public DateTime PaymentRequestCustomerExpirationTimestamp { get; set; }

        public DateTime? PaymentRequestApprovedTimestamp { get; set; }

        public DateTime? PaymentExecutionTimestamp { get; set; }
    }
}

using System;

namespace MAVN.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Used to trigger a single bonus
    /// </summary>
    public class BonusCustomerModel
    {
        /// <summary>
        /// CustomerId, must be filled when customer has acccount
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Email, must be filled when customer has no account
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Amount in cash
        /// </summary>
        public decimal? FiatAmount { get; set; }

        /// <summary>
        /// Used currency for payment
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The time when the payment was executed
        /// </summary>
        public DateTime? PaymentTimestamp { get; set; }

        /// <summary>
        /// The partner's id
        /// </summary>
        public string PartnerId { get; set; }

        /// <summary>
        /// The location's id
        /// </summary>
        public string ExternalLocationId { get; set; }

        /// <summary>
        /// Point of sale Id
        /// </summary>
        public string PosId { get; set; }
    }
}

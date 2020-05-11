using MAVN.Numerics;
using MAVN.Service.PartnersIntegration.Client.Enums;

namespace MAVN.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Model representing a customer balance response 
    /// </summary>
    public class CustomerBalanceResponseModel
    {
        /// <summary>
        /// The status of the retrieval
        /// </summary>
        public CustomerBalanceStatus Status { get; set; }

        /// <summary>
        /// The customer's tokens
        /// </summary>
        public Money18 Tokens { get; set; }

        /// <summary>
        /// Currency amount balance, calculated using the exchange rate for the selected currency
        /// </summary>
        public decimal FiatBalance { get; set; }

        /// <summary>
        /// Fiat currency for the fiat balance
        /// </summary>
        public string FiatCurrency { get; set; }
    }
}

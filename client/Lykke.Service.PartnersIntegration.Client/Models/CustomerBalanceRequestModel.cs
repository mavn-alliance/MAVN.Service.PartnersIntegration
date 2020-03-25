using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Model used to request info about customer's balance
    /// </summary>
    public class CustomerBalanceRequestModel
    {
        /// <summary>
        /// The hotel ID 
        /// </summary>
        public string PartnerId { get; set; }

        /// <summary>
        /// The hotel/location ID 
        /// </summary>
        public string ExternalLocationId { get; set; }

        /// <summary>
        /// Currency, used to calculate the token's exchange 
        /// </summary>
        public string Currency { get; set; }
    }
}

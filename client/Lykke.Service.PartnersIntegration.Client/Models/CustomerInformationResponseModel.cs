using Lykke.Service.PartnersIntegration.Client.Enums;

namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Model containing information about a customer
    /// </summary>
    public class CustomerInformationResponseModel
    {
        /// <summary>
        /// The status of the customer
        /// </summary>
        public CustomerInformationStatus Status { get; set; }

        /// <summary>
        /// The customer's Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The customer's Tier level
        /// </summary>
        public string TierLevel { get; set; }

        /// <summary>
        /// Customer's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Customer's last name
        /// </summary>
        public string LastName { get; set; }
    }
}

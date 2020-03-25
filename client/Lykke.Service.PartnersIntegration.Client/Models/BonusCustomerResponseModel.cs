using Lykke.Service.PartnersIntegration.Client.Enums;

namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Response containing the result of the bonus trigger for customer request
    /// </summary>
    public class BonusCustomerResponseModel
    {
        /// <summary>
        /// The sequential number of the sent bonus customer list item
        /// </summary>
        public int BonusCustomerSeqNumber { get; set; }

        /// <summary>
        /// The status of the response
        /// </summary>
        public BonusCustomerStatus Status { get; set; }

        /// <summary>
        /// The customer's Id
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// The customer's Email
        /// </summary>
        public string CustomerEmail { get; set; }
    }
}

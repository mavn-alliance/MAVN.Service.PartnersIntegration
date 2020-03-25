namespace Lykke.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Model containing information about the request for customer info
    /// </summary>
    public class CustomerInformationRequestModel
    {
        /// <summary>
        /// The customer's id. Required if other properties are null or empty.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The customer's email. Required if other properties are null or empty.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The customer's phone. Required if other properties are null or empty.
        /// </summary>
        public string Phone { get; set; }
    }
}

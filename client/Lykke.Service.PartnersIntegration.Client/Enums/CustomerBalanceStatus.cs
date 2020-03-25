namespace Lykke.Service.PartnersIntegration.Client.Enums
{
    /// <summary>
    /// Enum for customer balance status response
    /// </summary>
    public enum CustomerBalanceStatus
    {
        /// <summary>
        /// OK
        /// </summary>
        OK, 
        /// <summary>
        /// Customer not found
        /// </summary>
        CustomerNotFound,
        /// <summary>
        /// Partner not found
        /// </summary>
        PartnerNotFound,
        /// <summary>
        /// Location not found
        /// </summary>
        LocationNotFound,
        /// <summary>
        /// Invalid currency
        /// </summary>
        InvalidCurrency
    }
}

namespace MAVN.Service.PartnersIntegration.Client.Enums
{
    /// <summary>
    /// Error codes when sending a message from partner to customer
    /// </summary>
    public enum MessagesErrorCode
    {
        /// <summary>
        /// OK
        /// </summary>
        OK,
        /// <summary>
        /// CustomerNotFound
        /// </summary>
        CustomerNotFound,
        /// <summary>
        /// CustomerIsBlocked
        /// </summary>
        CustomerIsBlocked,
        /// <summary>
        /// PartnerNotFound
        /// </summary>
        PartnerNotFound,
        /// <summary>
        /// LocationNotFound
        /// </summary>
        LocationNotFound
    }
}

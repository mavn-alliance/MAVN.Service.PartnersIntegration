namespace MAVN.Service.PartnersIntegration.Client.Enums
{
    /// <summary>
    /// Available statuses for bonus trigger for customer response
    /// </summary>
    public enum BonusCustomerStatus
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
        /// Customer Id Does Not Match Email
        /// </summary>
        CustomerIdDoesNotMatchEmail,
        /// <summary>
        /// Invalid Currency
        /// </summary>
        InvalidCurrency,
        /// <summary>
        /// Invalid Fiat Amount
        /// </summary>
        InvalidFiatAmount,
        /// <summary>
        /// Invalid Payment Timestamp
        /// </summary>
        InvalidPaymentTimestamp,
        /// <summary>
        /// Technical problem occured
        /// </summary>
        TechnicalProblem
    }
}

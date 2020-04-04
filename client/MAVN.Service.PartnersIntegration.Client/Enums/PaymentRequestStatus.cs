namespace MAVN.Service.PartnersIntegration.Client.Enums
{
    /// <summary>
    /// Status of the payment request
    /// </summary>
    public enum PaymentRequestStatus
    {
        /// <summary>
        /// PaymentRequestNotFound
        /// </summary>
        PaymentRequestNotFound,
        /// <summary>
        /// PendingCustomerConfirmation
        /// </summary>
        PendingCustomerConfirmation,
        /// <summary>
        /// RejectedByCustomer
        /// </summary>
        RejectedByCustomer,
        /// <summary>
        /// PendingPartnerConfirmation
        /// </summary>
        PendingPartnerConfirmation,
        /// <summary>
        /// CancelledByPartner
        /// </summary>
        CancelledByPartner,
        /// <summary>
        /// PaymentExecuted
        /// </summary>
        PaymentExecuted,
        /// <summary>
        /// OperationFailed
        /// </summary>
        OperationFailed,
        /// <summary>
        /// RequestExpired
        /// </summary>
        RequestExpired,
        /// <summary>
        /// PaymentExpired
        /// </summary>
        PaymentExpired,
    }
}

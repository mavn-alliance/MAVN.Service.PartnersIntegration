namespace MAVN.Service.PartnersIntegration.Domain.Enums
{
    public enum PaymentRequestStatus
    {
        PaymentRequestNotFound,
        PendingCustomerConfirmation,
        RejectedByCustomer,
        PendingPartnerConfirmation,
        CancelledByPartner,
        PaymentExecuted,
        OperationFailed,
        RequestExpired,
        PaymentExpired,
    }
}

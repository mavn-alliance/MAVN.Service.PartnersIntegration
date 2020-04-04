namespace MAVN.Service.PartnersIntegration.Domain.Enums
{
    public enum BonusCustomerStatus
    {
        OK,
        CustomerNotFound,
        PartnerNotFound,
        LocationNotFound,
        CustomerIdDoesNotMatchEmail,
        InvalidCurrency,
        InvalidFiatAmount,
        InvalidPaymentTimestamp,
        TechnicalProblem
    }
}

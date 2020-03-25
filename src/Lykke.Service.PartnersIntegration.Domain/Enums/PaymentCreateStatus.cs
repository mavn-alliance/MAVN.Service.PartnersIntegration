namespace Lykke.Service.PartnersIntegration.Domain.Enums
{
    public enum PaymentCreateStatus
    {
        OK,
        CustomerNotFound,
        CustomerIsBlocked,
        PartnerNotFound,
        LocationNotFound,
        InvalidCurrency,
        CannotPassBothFiatAndTokensAmount,
        EitherFiatOrTokensAmountShouldBePassed,
        InvalidTokensAmount,
        InvalidFiatAmount,
        InvalidTotalBillAmount,
        InternalTechnicalError,
    }
}

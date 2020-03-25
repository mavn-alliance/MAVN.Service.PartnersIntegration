namespace Lykke.Service.PartnersIntegration.Client.Enums
{
    /// <summary>
    /// Response status for creating a payment request
    /// </summary>
    public enum PaymentCreateStatus
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
        LocationNotFound,
        /// <summary>
        /// InvalidCurrency
        /// </summary>
        InvalidCurrency,
        /// <summary>
        /// Cannot pass both fiat and tokens amount
        /// </summary>
        CannotPassBothFiatAndTokensAmount,
        /// <summary>
        /// Either Fiat or Tokens amount should be passed
        /// </summary>
        EitherFiatOrTokensAmountShouldBePassed,
        /// <summary>
        /// Invalid Tokens amount
        /// </summary>
        InvalidTokensAmount,
        /// <summary>
        /// Invalid Fiat amount
        /// </summary>
        InvalidFiatAmount,
        /// <summary>
        /// Invalid total bill amount
        /// </summary>
        InvalidTotalBillAmount,
        /// <summary>
        /// Internal technical error 
        /// </summary>
        InternalTechnicalError,
    }
}

using System;
using FluentValidation;
using MAVN.Service.PartnersIntegration.Client.Models;

namespace MAVN.Service.PartnersIntegration.Validation
{
    public class PaymentsCreateRequestModelValidator : AbstractValidator<PaymentsCreateRequestModel>
    {
        public PaymentsCreateRequestModelValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x)
                .Must(x => x.FiatAmount.HasValue || x.TokensAmount.HasValue)
                .WithMessage("Fiat or Token amount required");

            RuleFor(x => x.TotalFiatAmount)
                .Must(x => x.HasValue)
                .WithMessage("Total fiat amount required")
                .Must(x => x.Value > 0)
                .WithMessage("TotalFiatAmount must be a positive number");

            RuleFor(x => x.Currency)
                .NotNull()
                .NotEmpty()
                .WithMessage("Currency required")
                .Must(x => x.Length <= 20)
                .WithMessage("Currency length must not be bigger than 20");

            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.PartnerId)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.ExternalLocationId)
                .NotEmpty()
                .NotNull();

            When(x => !x.TokensAmount.HasValue, () =>
            {
                RuleFor(x => x.FiatAmount)
                    .Must(x => x.HasValue && x.Value > 0)
                    .WithMessage("FiatAmount must be a positive number");
            });

            When(x => x.ExpirationTimeoutInSeconds.HasValue, () =>
            {
                RuleFor(x => x.ExpirationTimeoutInSeconds)
                    .Must(x => x.HasValue && x.Value > 0)
                    .WithMessage("ExpirationTimeoutInSeconds must be a positive number");
            });

            When(x => !x.FiatAmount.HasValue, () =>
            {
                RuleFor(x => x.TokensAmount)
                    .Must(x => x.HasValue && x.Value > 0)
                    .WithMessage("TokensAmount must be a positive number");
            });

            When(x => !string.IsNullOrEmpty(x.PaymentInfo), () =>
            {
                RuleFor(x => x.PaymentInfo)
                    .MaximumLength(5120)
                    .WithMessage("payment info length must not be bigger than 5120");
            });

            When(x => !string.IsNullOrWhiteSpace(x.PaymentProcessedCallbackUrl), () =>
            {
                RuleFor(x => x.PaymentProcessedCallbackUrl)
                    .Must(x => x.Length <= 512)
                    .WithMessage("PaymentProcessedCallbackUrl length must not be bigger than 512");

                RuleFor(x => x.RequestAuthToken)
                    .NotEmpty()
                    .NotNull()
                    .WithMessage("RequestAuthToken required when PaymentProcessedCallbackUrl passed.")
                    .Must(x => x.Length <= 100)
                    .WithMessage("RequestAuthToken length must not be bigger than 100");
            });
        }
    }
}

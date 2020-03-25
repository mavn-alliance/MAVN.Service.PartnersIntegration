using FluentValidation;
using Lykke.Service.PartnersIntegration.Client.Models;

namespace Lykke.Service.PartnersIntegration.Validation
{
    public class CustomerInformationRequestModelValidator : AbstractValidator<CustomerInformationRequestModel>
    {
        public CustomerInformationRequestModelValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x).Must(x => !string.IsNullOrWhiteSpace(x.Id) ||
                                      !string.IsNullOrWhiteSpace(x.Email) ||
                                      !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("Id, Email or Phone required");

            When(x => x.Id != null, () =>
            {
                RuleFor(x => x.Id).Must(x => x.Length <= 100)
                    .WithMessage("Id must not be more than 100 characters");

            });

            When(x => x.Email != null, () =>
            {
                RuleFor(x => x.Email).Must(x => x.Length <= 100)
                    .WithMessage("Id must not be more than 100 characters");
            });

            When(x => x.Phone != null, () => 
            {
                RuleFor(x => x.Phone).Must(x => x.Length <= 50)
                    .WithMessage("Phone must not be more than 50 characters");
            });
        }
    }
}

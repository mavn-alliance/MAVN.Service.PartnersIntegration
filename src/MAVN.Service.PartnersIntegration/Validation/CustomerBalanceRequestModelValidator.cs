using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MAVN.Service.PartnersIntegration.Client.Models;

namespace MAVN.Service.PartnersIntegration.Validation
{
    public class CustomerBalanceRequestModelValidator : AbstractValidator<CustomerBalanceRequestModel>
    {
        public CustomerBalanceRequestModelValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.PartnerId)
                .NotEmpty()
                .NotNull()
                .Length(1, 100);

            RuleFor(x => x.Currency)
                .NotNull()
                .NotEmpty()
                .WithMessage("Currency required")
                .Must(x => x.Length <= 20)
                .WithMessage("Currency length must not be bigger than 20");

            RuleFor(x => x.ExternalLocationId)
                .Length(1, 100);
        }
    }
}

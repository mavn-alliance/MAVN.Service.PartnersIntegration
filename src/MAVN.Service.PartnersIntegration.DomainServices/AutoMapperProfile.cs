using System;
using AutoMapper;
using MAVN.Service.EligibilityEngine.Client.Enums;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersPayments.Client.Enums;
using MAVN.Service.PartnersIntegration.Domain.Enums;
using MAVN.Service.PartnersPayments.Client.Models;

namespace MAVN.Service.PartnersIntegration.DomainServices
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<EligibilityEngineErrors, CustomerBalanceStatus>()
                .ConvertUsing(new EligibilityEngineErrorsToCustomerBalanceStatusConverter());

            CreateMap<PaymentsCreateRequest, MessagesPostRequest>()
                .ForMember(dest => dest.SendPushNotification, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.PaymentInfo));

            CreateMap<PaymentsCreateRequest, PaymentRequestModel>()
                .ForMember(dest => dest.TotalBillAmount, opt => opt.MapFrom(o => o.TotalFiatAmount))
                .ForMember(dest => dest.CustomerExpirationInSeconds, opt => opt.MapFrom(o => o.ExpirationTimeoutInSeconds))
                .ForMember(dest => dest.LocationId, opt => opt.Ignore())
                .ForMember(dest => dest.PartnerMessageId, opt => opt.Ignore());

            CreateMap<PaymentRequestErrorCodes, PaymentCreateStatus>()
                .ConvertUsing(new PaymentRequestErrorCodesToPaymentCreateStatusConverter());
        }
    }

    internal class
        EligibilityEngineErrorsToCustomerBalanceStatusConverter : ITypeConverter<EligibilityEngineErrors,
            CustomerBalanceStatus>
    {
        /// <summary>Performs conversion from source to destination type</summary>
        /// <param name="source">Source object, <see cref="EligibilityEngineErrors"/></param>
        /// <param name="destination">Destination object, <see cref="CustomerBalanceStatus"/></param>
        /// <param name="context">Resolution context</param>
        /// <returns>Destination object</returns>
        public CustomerBalanceStatus Convert(EligibilityEngineErrors source, CustomerBalanceStatus destination,
            ResolutionContext context)
        {
            switch (source)
            {
                case EligibilityEngineErrors.PartnerNotFound:
                    return CustomerBalanceStatus.PartnerNotFound;
                case EligibilityEngineErrors.CustomerNotFound:
                    return CustomerBalanceStatus.CustomerNotFound;
                case EligibilityEngineErrors.ConversionRateNotFound:
                case EligibilityEngineErrors.EarnRuleNotFound:
                case EligibilityEngineErrors.SpendRuleNotFound:
                    return CustomerBalanceStatus.InvalidCurrency;
                case EligibilityEngineErrors.None:
                    return CustomerBalanceStatus.OK;
                default:
                    throw new ArgumentException($"Could not convert {source} to {destination}");
            }
        }
    }

    internal class
        PaymentRequestErrorCodesToPaymentCreateStatusConverter : ITypeConverter<PaymentRequestErrorCodes,
            PaymentCreateStatus>
    {
        /// <summary>Performs conversion from source to destination type</summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object</param>
        /// <param name="context">Resolution context</param>
        /// <returns>Destination object</returns>
        public PaymentCreateStatus Convert(PaymentRequestErrorCodes source, PaymentCreateStatus destination,
            ResolutionContext context)
        {
            switch (source)
            {
                case PaymentRequestErrorCodes.None:
                    return PaymentCreateStatus.OK;
                case PaymentRequestErrorCodes.CustomerDoesNotExist:
                    return PaymentCreateStatus.CustomerNotFound;
                case PaymentRequestErrorCodes.CustomerWalletBlocked:
                    return PaymentCreateStatus.CustomerIsBlocked;
                case PaymentRequestErrorCodes.CannotPassBothFiatAndTokensAmount:
                    return PaymentCreateStatus.CannotPassBothFiatAndTokensAmount;
                case PaymentRequestErrorCodes.InvalidCurrency:
                    return PaymentCreateStatus.InvalidCurrency;
                case PaymentRequestErrorCodes.EitherFiatOrTokensAmountShouldBePassed:
                    return PaymentCreateStatus.EitherFiatOrTokensAmountShouldBePassed;
                case PaymentRequestErrorCodes.InvalidTokensAmount:
                    return PaymentCreateStatus.InvalidTokensAmount;
                case PaymentRequestErrorCodes.InvalidFiatAmount:
                    return PaymentCreateStatus.InvalidFiatAmount;
                case PaymentRequestErrorCodes.InvalidTotalBillAmount:
                    return PaymentCreateStatus.InvalidTotalBillAmount;
                case PaymentRequestErrorCodes.PartnerIdIsNotAValidGuid:
                    return PaymentCreateStatus.PartnerNotFound;
                case PaymentRequestErrorCodes.PartnerDoesNotExist:
                    return PaymentCreateStatus.PartnerNotFound;
                case PaymentRequestErrorCodes.NoSuchLocationForThisPartner:
                    return PaymentCreateStatus.LocationNotFound;
                case PaymentRequestErrorCodes.InvalidTokensOrCurrencyRateInPartner:
                default:
                    return PaymentCreateStatus.InternalTechnicalError;
            }
        }
    }
}

using AutoMapper;
using Lykke.Service.PartnersIntegration.Client.Models;
using Lykke.Service.PartnersIntegration.Domain.Models;

namespace Lykke.Service.PartnersIntegration.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //Customer
            CreateMap<CustomerInformationRequestModel, CustomerInformationRequest>();
            CreateMap<CustomerInformationResponse, CustomerInformationResponseModel>()
                .ForMember(src => src.Id, opt => opt.MapFrom(o => o.CustomerId));

            CreateMap<CustomerBalanceRequestModel, CustomerBalanceRequest>();
            CreateMap<CustomerBalanceResponse, CustomerBalanceResponseModel>();

            //Referral
            CreateMap<ReferralInformationRequestModel, ReferralInformationRequest>();
            CreateMap<ReferralInformationResponse, ReferralInformationResponseModel>();
            CreateMap<Domain.Models.Referral, ReferralModel>();

            //Bonus
            CreateMap<BonusCustomerModel, BonusCustomerRequest>();
            CreateMap<BonusCustomerResponse, BonusCustomerResponseModel>();

            //Payments
            CreateMap<PaymentRequestStatusResponse, PaymentRequestStatusResponseModel>();

            CreateMap<PaymentsCreateRequestModel, PaymentsCreateRequest>();
            CreateMap<PaymentsCreateResponse, PaymentsCreateResponseModel>();

            CreateMap<PaymentsExecuteRequestModel, PaymentsExecuteRequest>();
            CreateMap<PaymentsExecuteResponse, PaymentsExecuteResponseModel>();

            //Messages
            CreateMap<MessagesPostRequestModel, MessagesPostRequest>();
            CreateMap<MessagesPostResponse, MessagesPostResponseModel>();

            CreateMap<MessageGetResponse, MessageGetResponseModel>();
        }
    }
}

using AutoMapper;
using Lykke.Service.PartnersIntegration.Domain.Models;
using Lykke.Service.PartnersIntegration.MsSqlRepositories.Entities;

namespace Lykke.Service.PartnersIntegration.MsSqlRepositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MessagesPostRequest, MessageEntity>()
                .ForMember(src => src.CreationTimestamp, opt => opt.Ignore())
                .ForMember(src => src.Id, opt => opt.Ignore());

            CreateMap<MessageEntity, MessageGetResponse>()
                .ForMember(src => src.Message, opt => opt.Ignore());

            CreateMap<PaymentProcessedCallbackUrl, PaymentProcessedCallbackUrlEntity>()
                .ForMember(src => src.Id, opt => opt.Ignore());

            CreateMap<PaymentProcessedCallbackUrlEntity, PaymentProcessedCallbackUrl>();
        }
    }
}

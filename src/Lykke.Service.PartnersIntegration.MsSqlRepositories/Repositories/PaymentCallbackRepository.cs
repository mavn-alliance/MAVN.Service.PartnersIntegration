using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.MsSql;
using Lykke.Service.PartnersIntegration.Domain.Models;
using Lykke.Service.PartnersIntegration.Domain.Repositories;
using Lykke.Service.PartnersIntegration.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Service.PartnersIntegration.MsSqlRepositories.Repositories
{
    public class PaymentCallbackRepository : IPaymentCallbackRepository
    {
        private readonly MsSqlContextFactory<PartnersIntegrationContext> _msSqlContextFactory;
        private readonly IMapper _mapper;

        public PaymentCallbackRepository(MsSqlContextFactory<PartnersIntegrationContext> msSqlContextFactory,
            IMapper mapper)
        {
            _msSqlContextFactory = msSqlContextFactory;
            _mapper = mapper;
        }

        public async Task InsertAsync(PaymentProcessedCallbackUrl contract)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = _mapper.Map<PaymentProcessedCallbackUrlEntity>(contract);

                context.Add(entity);

                await context.SaveChangesAsync();
            }
        }

        public async Task<PaymentProcessedCallbackUrl> GetByIdAsync(string paymentRequestId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = await context.PaymentProcessedCallbackUrls.FirstOrDefaultAsync(m => m.PaymentRequestId == paymentRequestId);

                return _mapper.Map<PaymentProcessedCallbackUrl>(entity);
            }
        }
    }
}

using System.Threading.Tasks;
using AutoMapper;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.Domain.Repositories;
using MAVN.Service.PartnersIntegration.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.PartnersIntegration.MsSqlRepositories.Repositories
{
    public class PaymentCallbackRepository : IPaymentCallbackRepository
    {
        private readonly PostgreSQLContextFactory<PartnersIntegrationContext> _msSqlContextFactory;
        private readonly IMapper _mapper;

        public PaymentCallbackRepository(PostgreSQLContextFactory<PartnersIntegrationContext> msSqlContextFactory,
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

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.Domain.Repositories;
using MAVN.Service.PartnersIntegration.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.PartnersIntegration.MsSqlRepositories.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        private readonly PostgreSQLContextFactory<PartnersIntegrationContext> _msSqlContextFactory;
        private readonly IMapper _mapper;

        public MessagesRepository(PostgreSQLContextFactory<PartnersIntegrationContext> msSqlContextFactory,
            IMapper mapper)
        {
            _msSqlContextFactory = msSqlContextFactory;
            _mapper = mapper;
        }

        public async Task<Guid> InsertAsync(MessagesPostRequest contract)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = _mapper.Map<MessageEntity>(contract);

                entity.CreationTimestamp = DateTime.UtcNow;

                context.Add(entity);

                await context.SaveChangesAsync();

                return entity.Id;
            }
        }

        public async Task<MessageGetResponse> GetByIdAsync(string partnerMessageId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = await context.Messages.FirstOrDefaultAsync(m => m.Id.ToString() == partnerMessageId);

                return _mapper.Map<MessageGetResponse>(entity);
            }
        }

        public async Task DeleteMessageAsync(string partnerMessageId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = await context.Messages.FirstOrDefaultAsync(m => m.Id.ToString() == partnerMessageId);

                if (entity != null)
                {
                    context.Remove(entity);

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

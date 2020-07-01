using System.Data.Common;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.PartnersIntegration.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.PartnersIntegration.MsSqlRepositories
{
    public class PartnersIntegrationContext : PostgreSQLContext
    {
        private const string Schema = "partners_integration";

        public DbSet<MessageEntity> Messages { get; set; }
        public DbSet<PaymentProcessedCallbackUrlEntity> PaymentProcessedCallbackUrls { get; set; }

        // empty constructor needed for migrations
        public PartnersIntegrationContext() : base(Schema)
        {
        }

        public PartnersIntegrationContext(string connectionString, bool isTraceEnabled)
            : base(Schema, connectionString, isTraceEnabled)
        {
        }

        //Needed constructor for using InMemoryDatabase for tests
        public PartnersIntegrationContext(DbContextOptions options)
            : base(Schema, options)
        {
        }

        public PartnersIntegrationContext(DbConnection dbConnection)
            : base(Schema, dbConnection)
        {
        }

        protected override void OnMAVNModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentProcessedCallbackUrlEntity>()
                .HasIndex(b => b.PaymentRequestId)
                .IsUnique();
        }
    }
}

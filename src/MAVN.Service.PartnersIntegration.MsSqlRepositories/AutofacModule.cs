using Autofac;
using Lykke.Common.MsSql;
using MAVN.Service.PartnersIntegration.Domain.Repositories;
using MAVN.Service.PartnersIntegration.MsSqlRepositories.Repositories;

namespace MAVN.Service.PartnersIntegration.MsSqlRepositories
{
    public class AutofacModule : Module
    {
        private readonly string _connectionString;

        public AutofacModule(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMsSql(
                _connectionString,
                connString => new PartnersIntegrationContext(connString, false),
                dbConn => new PartnersIntegrationContext(dbConn));

            builder.RegisterType<MessagesRepository>()
                .As<IMessagesRepository>()
                .SingleInstance();

            builder.RegisterType<PaymentCallbackRepository>()
                .As<IPaymentCallbackRepository>()
                .SingleInstance();
        }
    }
}

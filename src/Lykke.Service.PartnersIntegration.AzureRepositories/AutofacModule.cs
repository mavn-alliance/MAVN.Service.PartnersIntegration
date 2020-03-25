using System;
using Autofac;
using AzureStorage.Blob;
using Lykke.Service.PartnersIntegration.Domain.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.PartnersIntegration.AzureRepositories
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<string> _connectionString;

        public AutofacModule(IReloadingManager<string> connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(container =>
                    new MessageContentRepository(AzureBlobStorage.Create(_connectionString, TimeSpan.FromMinutes(15))))
                .As<IMessageContentRepository>()
                .SingleInstance();
        }
    }
}

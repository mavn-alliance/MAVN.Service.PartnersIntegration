using Autofac;
using JetBrains.Annotations;
using Lykke.Sdk;
using MAVN.Service.PartnersIntegration.Services;
using MAVN.Service.PartnersIntegration.Settings;
using Lykke.SettingsReader;

namespace MAVN.Service.PartnersIntegration.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var settings = _appSettings.CurrentValue;

            builder.RegisterModule(new DomainServices.AutofacModule(
                settings.CustomerProfileServiceClient,
                settings.PrivateBlockchainFacadeClient,
                settings.ReferralServiceClient,
                settings.PartnersPaymentsServiceClient,
                settings.PartnerManagementServiceClient,
                settings.TiersServiceClient,
                settings.CustomerManagementServiceClient,
                settings.EligibilityEngineServiceClient,
                settings.PartnersIntegrationService.PartnerPaymentClient.ExternalPartnerPaymentConnectionRetries,
                settings.PartnersIntegrationService.Notifications.PushNotifications.PartnerMessageTemplateId));

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterModule(new MsSqlRepositories.AutofacModule(
                _appSettings.CurrentValue.PartnersIntegrationService.Db.DataConnString
                ));

            builder.RegisterModule(new AzureRepositories.AutofacModule(
                _appSettings.Nested(x => x.PartnersIntegrationService.Db.MessageContentConnString)
            ));
        }
    }
}

using Autofac;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.NotificationSystem.SubscriberContract;
using Lykke.Service.PartnersIntegration.Contract;
using Lykke.Service.PartnersIntegration.DomainServices.Subscribers;
using Lykke.Service.PartnersIntegration.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.PartnersIntegration.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private const string QueueName = "partnersintegration";
        private const string PartnersPaymentStatusUpdatedExchangeName = "lykke.wallet.partnerspaymentstatusupdated";
        private const string BonusCustomerExchangeName = "lykke.partnersintegration.bonuscustomertrigger";
        private const string PushNotificationExchangeName = "notificationsystem.command.pushnotification";

        private readonly RabbitMqSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.Rabbit;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterJsonRabbitPublisher<BonusCustomerTriggerEvent>(
                _settings.ConnectionString,
                BonusCustomerExchangeName);

            builder.RegisterJsonRabbitPublisher<PushNotificationEvent>(
                _settings.ConnectionString,
                PushNotificationExchangeName);

            builder.RegisterType<PartnersPaymentStatusUpdatedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _settings.WalletConnectionString)
                .WithParameter("exchangeName", PartnersPaymentStatusUpdatedExchangeName)
                .WithParameter("queueName", QueueName);
        }
    }
}

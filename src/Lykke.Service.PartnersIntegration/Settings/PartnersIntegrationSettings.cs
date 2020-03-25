using JetBrains.Annotations;
using Lykke.Service.PartnersIntegration.Settings.Notifications;

namespace Lykke.Service.PartnersIntegration.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PartnersIntegrationSettings
    {
        public DbSettings Db { get; set; }

        public NotificationsSettings Notifications { get; set; }

        public PartnerPaymentClientSettings PartnerPaymentClient { get; set; }
    }
}

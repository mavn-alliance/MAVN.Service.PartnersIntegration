using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.CustomerManagement.Client;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.PartnersPayments.Client;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.Referral.Client;
using Lykke.Service.Tiers.Client;

namespace MAVN.Service.PartnersIntegration.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public PartnersIntegrationSettings PartnersIntegrationService { get; set; }

        public CustomerProfileServiceClientSettings CustomerProfileServiceClient { get; set; }

        public PrivateBlockchainFacadeServiceClientSettings PrivateBlockchainFacadeClient { get; set; }

        public RabbitMqSettings Rabbit { get; set; }

        public ReferralServiceClientSettings ReferralServiceClient { get; set; }

        public PartnersPaymentsServiceClientSettings PartnersPaymentsServiceClient { get; set; }

        public PartnerManagementServiceClientSettings PartnerManagementServiceClient { get; set; }

        public TiersServiceClientSettings TiersServiceClient { get; set; }

        public CustomerManagementServiceClientSettings CustomerManagementServiceClient { get; set; }

        public EligibilityEngineServiceClientSettings EligibilityEngineServiceClient { get; set; }
    }
}

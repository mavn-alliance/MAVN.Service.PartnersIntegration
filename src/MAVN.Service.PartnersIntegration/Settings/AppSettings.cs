using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using MAVN.Service.CustomerManagement.Client;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.EligibilityEngine.Client;
using MAVN.Service.PartnersPayments.Client;
using MAVN.Service.PartnerManagement.Client;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.Referral.Client;
using MAVN.Service.Tiers.Client;

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

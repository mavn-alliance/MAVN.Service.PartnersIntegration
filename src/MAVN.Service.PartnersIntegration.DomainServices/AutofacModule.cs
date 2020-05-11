using Autofac;
using MAVN.Service.CustomerManagement.Client;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.EligibilityEngine.Client;
using MAVN.Service.PartnerManagement.Client;
using MAVN.Service.PartnersIntegration.Domain.Helpers;
using MAVN.Service.PartnersIntegration.Domain.Services;
using MAVN.Service.PartnersIntegration.DomainServices.Services;
using MAVN.Service.PartnersPayments.Client;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.Referral.Client;
using MAVN.Service.Tiers.Client;

namespace MAVN.Service.PartnersIntegration.DomainServices
{
    public class AutofacModule : Module
    {
        private readonly CustomerProfileServiceClientSettings _customerProfileServiceClientSettings;
        private readonly PrivateBlockchainFacadeServiceClientSettings _privateBlockchainFacadeServiceClientSettings;
        private readonly ReferralServiceClientSettings _referralServiceClientSettings;
        private readonly PartnersPaymentsServiceClientSettings _partnersPaymentsServiceClientSettings;
        private readonly PartnerManagementServiceClientSettings _partnerManagementServiceClientSettings;
        private readonly TiersServiceClientSettings _tiersServiceClientSettings;
        private readonly CustomerManagementServiceClientSettings _customerManagementServiceClientSettings;
        private readonly EligibilityEngineServiceClientSettings _eligibilityEngineServiceClientSettings;
        private readonly int _externalPartnerPaymentConnectionRetries;
        private readonly string _partnerMessageTemplateId;

        public AutofacModule(
            CustomerProfileServiceClientSettings customerProfileServiceClientSettings,
            PrivateBlockchainFacadeServiceClientSettings privateBlockchainFacadeServiceClientSettings,
            ReferralServiceClientSettings referralServiceClientSettings,
            PartnersPaymentsServiceClientSettings partnersPaymentsServiceClientSettings,
            PartnerManagementServiceClientSettings partnerManagementServiceClientSettings,
            TiersServiceClientSettings tiersServiceClientSettings, 
            CustomerManagementServiceClientSettings customerManagementServiceClientSettings,
            EligibilityEngineServiceClientSettings eligibilityEngineServiceClientSettings,
            int externalPartnerPaymentConnectionRetries,
            string partnerMessageTemplateId)
        {
            _customerProfileServiceClientSettings = customerProfileServiceClientSettings;
            _privateBlockchainFacadeServiceClientSettings = privateBlockchainFacadeServiceClientSettings;
            _referralServiceClientSettings = referralServiceClientSettings;
            _partnersPaymentsServiceClientSettings = partnersPaymentsServiceClientSettings;
            _partnerManagementServiceClientSettings = partnerManagementServiceClientSettings;
            _tiersServiceClientSettings = tiersServiceClientSettings;
            _customerManagementServiceClientSettings = customerManagementServiceClientSettings;
            _externalPartnerPaymentConnectionRetries = externalPartnerPaymentConnectionRetries;
            _eligibilityEngineServiceClientSettings = eligibilityEngineServiceClientSettings;
            _partnerMessageTemplateId = partnerMessageTemplateId;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterCustomerProfileClient(_customerProfileServiceClientSettings);
            builder.RegisterPrivateBlockchainFacadeClient(_privateBlockchainFacadeServiceClientSettings, null);
            builder.RegisterReferralClient(_referralServiceClientSettings, null);
            builder.RegisterPartnersPaymentsClient(_partnersPaymentsServiceClientSettings, null);
            builder.RegisterPartnerManagementClient(_partnerManagementServiceClientSettings, null);
            builder.RegisterTiersClient(_tiersServiceClientSettings);
            builder.RegisterCustomerManagementClient(_customerManagementServiceClientSettings, null);
            builder.RegisterEligibilityEngineClient(_eligibilityEngineServiceClientSettings, null);

            builder.RegisterType<CustomersService>()
                .As<ICustomersService>()
                .SingleInstance();

            builder.RegisterType<ReferralsService>()
                .As<IReferralsService>()
                .SingleInstance();

            builder.RegisterType<BonusService>()
                .As<IBonusService>()
                .SingleInstance();

            builder.RegisterType<PaymentsService>()
                .As<IPaymentsService>()
                .SingleInstance()
                .WithParameter("externalPartnerPaymentConnectionRetries", _externalPartnerPaymentConnectionRetries);

            builder.RegisterType<MessagesService>()
                .As<IMessagesService>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_partnerMessageTemplateId));

            builder.RegisterType<PartnerAndLocationHelper>()
                .As<IPartnerAndLocationHelper>()
                .SingleInstance();
        }
    }
}

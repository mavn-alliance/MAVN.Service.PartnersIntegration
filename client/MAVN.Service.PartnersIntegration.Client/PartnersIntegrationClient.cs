using Lykke.HttpClientGenerator;
using MAVN.Service.PartnersIntegration.Client.Api;

namespace MAVN.Service.PartnersIntegration.Client
{
    /// <summary>
    /// PartnersIntegration API aggregating interface.
    /// </summary>
    public class PartnersIntegrationClient : IPartnersIntegrationClient
    {
        /// <summary>Interface to PartnersIntegration CustomersApi.</summary>
        public ICustomersApi CustomersApi { get; private set; }

        /// <summary>Interface to PartnersIntegration ReferralsApi</summary>
        public IReferralsApi ReferralsApi { get; }

        /// <summary>Interface to PartnersIntegration BonusApi</summary>
        public IBonusApi BonusApi { get; }

        /// <summary>Interface to PartnersIntegration PaymentsApi</summary>
        public IPaymentsApi PaymentsApi { get; }

        /// <summary>Interface to PartnersIntegration MessagesApi</summary>
        public IMessagesApi MessagesApi { get; }

        /// <summary>C-tor</summary>
        public PartnersIntegrationClient(IHttpClientGenerator httpClientGenerator)
        {
            CustomersApi = httpClientGenerator.Generate<ICustomersApi>();
            ReferralsApi = httpClientGenerator.Generate<IReferralsApi>();
            BonusApi = httpClientGenerator.Generate<IBonusApi>();
            PaymentsApi = httpClientGenerator.Generate<IPaymentsApi>();
            MessagesApi = httpClientGenerator.Generate<IMessagesApi>();
        }
    }
}

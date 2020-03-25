using JetBrains.Annotations;
using Lykke.Service.PartnersIntegration.Client.Api;

namespace Lykke.Service.PartnersIntegration.Client
{
    /// <summary>
    /// PartnersIntegration client interface.
    /// </summary>
    [PublicAPI]
    public interface IPartnersIntegrationClient
    {
        /// <summary>Application CustomersApi interface</summary>
        ICustomersApi CustomersApi { get; }

        /// <summary>Application ReferralsApi interface</summary>
        IReferralsApi ReferralsApi { get; }

        /// <summary>Application BonusApi interface</summary>
        IBonusApi BonusApi { get; }

        /// <summary>Application PaymentsApi interface</summary>
        IPaymentsApi PaymentsApi { get; }

        /// <summary>Application MessagesApi interface</summary>
        IMessagesApi MessagesApi { get; }
    }
}

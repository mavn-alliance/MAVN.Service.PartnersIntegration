using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MAVN.Service.PartnersIntegration.Client.Models;
using Refit;

namespace MAVN.Service.PartnersIntegration.Client.Api
{
    /// <summary>
    /// PartnersIntegration Bonus client API interface.
    /// </summary>
    [PublicAPI]
    public interface IBonusApi
    {
        /// <summary>
        /// Used to trigger a bonus to a customer
        /// </summary>
        /// <param name="model">Information about the bonus and customer</param>
        /// <returns>Status of the trigger</returns>
        [Post("/api/bonus/customers")]
        Task<List<BonusCustomerResponseModel>> TriggerBonusToCustomers(BonusCustomersRequestModel model);
    }
}

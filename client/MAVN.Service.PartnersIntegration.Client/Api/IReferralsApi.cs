using System.Threading.Tasks;
using JetBrains.Annotations;
using MAVN.Service.PartnersIntegration.Client.Models;
using Refit;

namespace MAVN.Service.PartnersIntegration.Client.Api
{
    /// <summary>
    /// PartnersIntegration Referrals client API interface.
    /// </summary>
    [PublicAPI]
    public interface IReferralsApi
    {
        /// <summary>
        /// Endpoint for retrieving referral information
        /// </summary>
        /// <param name="model">Filter request data</param>
        /// <returns></returns>
        [Post("/api/referrals/query")]
        Task<ReferralInformationResponseModel> ReferralInformation(ReferralInformationRequestModel model);
    }
}

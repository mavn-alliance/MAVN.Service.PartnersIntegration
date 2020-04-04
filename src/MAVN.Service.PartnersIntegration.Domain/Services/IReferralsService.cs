using System.Threading.Tasks;
using MAVN.Service.PartnersIntegration.Domain.Models;

namespace MAVN.Service.PartnersIntegration.Domain.Services
{
    public interface IReferralsService
    {
        Task<ReferralInformationResponse> GetReferralInformationAsync(ReferralInformationRequest contract);
    }
}

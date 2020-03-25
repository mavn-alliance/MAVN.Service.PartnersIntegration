using System.Threading.Tasks;
using Lykke.Service.PartnersIntegration.Domain.Models;

namespace Lykke.Service.PartnersIntegration.Domain.Services
{
    public interface IReferralsService
    {
        Task<ReferralInformationResponse> GetReferralInformationAsync(ReferralInformationRequest contract);
    }
}

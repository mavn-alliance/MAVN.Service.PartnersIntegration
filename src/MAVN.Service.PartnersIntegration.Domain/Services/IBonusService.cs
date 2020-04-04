using System.Collections.Generic;
using System.Threading.Tasks;
using MAVN.Service.PartnersIntegration.Domain.Models;

namespace MAVN.Service.PartnersIntegration.Domain.Services
{
    public interface IBonusService
    {
        Task<List<BonusCustomerResponse>> TriggerBonusToCustomersAsync(List<BonusCustomerRequest> contracts);
    }
}

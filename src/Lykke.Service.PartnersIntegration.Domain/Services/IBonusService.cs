using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PartnersIntegration.Domain.Models;

namespace Lykke.Service.PartnersIntegration.Domain.Services
{
    public interface IBonusService
    {
        Task<List<BonusCustomerResponse>> TriggerBonusToCustomersAsync(List<BonusCustomerRequest> contracts);
    }
}

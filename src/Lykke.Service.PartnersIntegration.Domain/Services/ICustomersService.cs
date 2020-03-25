
using System.Threading.Tasks;
using Lykke.Service.PartnersIntegration.Domain.Models;

namespace Lykke.Service.PartnersIntegration.Domain.Services
{
    public interface ICustomersService
    {
        Task<CustomerInformationResponse> GetCustomerInformationAsync(CustomerInformationRequest contract);

        Task<CustomerBalanceResponse> GetCustomerBalanceAsync(string customerId,
            CustomerBalanceRequest contract);
    }
}

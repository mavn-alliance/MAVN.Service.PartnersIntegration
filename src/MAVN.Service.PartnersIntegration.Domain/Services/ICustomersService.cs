
using System.Threading.Tasks;
using MAVN.Service.PartnersIntegration.Domain.Models;

namespace MAVN.Service.PartnersIntegration.Domain.Services
{
    public interface ICustomersService
    {
        Task<CustomerInformationResponse> GetCustomerInformationAsync(CustomerInformationRequest contract);

        Task<CustomerBalanceResponse> GetCustomerBalanceAsync(string customerId,
            CustomerBalanceRequest contract);
    }
}

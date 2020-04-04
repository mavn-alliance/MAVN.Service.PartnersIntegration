using System.Threading.Tasks;
using MAVN.Service.PartnersIntegration.Domain.Models;

namespace MAVN.Service.PartnersIntegration.Domain.Repositories
{
    public interface IPaymentCallbackRepository
    {
        Task InsertAsync(PaymentProcessedCallbackUrl contract);

        Task<PaymentProcessedCallbackUrl> GetByIdAsync(string paymentRequestId);
    }
}

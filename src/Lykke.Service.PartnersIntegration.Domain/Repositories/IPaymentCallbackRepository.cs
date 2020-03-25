using System.Threading.Tasks;
using Lykke.Service.PartnersIntegration.Domain.Models;

namespace Lykke.Service.PartnersIntegration.Domain.Repositories
{
    public interface IPaymentCallbackRepository
    {
        Task InsertAsync(PaymentProcessedCallbackUrl contract);

        Task<PaymentProcessedCallbackUrl> GetByIdAsync(string paymentRequestId);
    }
}

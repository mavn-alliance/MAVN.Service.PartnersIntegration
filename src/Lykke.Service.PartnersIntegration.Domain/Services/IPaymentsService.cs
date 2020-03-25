using System.Threading.Tasks;
using Lykke.Service.PartnersIntegration.Domain.Models;
using Lykke.Service.PartnersPayments.Contract;

namespace Lykke.Service.PartnersIntegration.Domain.Services
{
    public interface IPaymentsService
    {
        Task<PaymentsCreateResponse> CreatePaymentRequestAsync(PaymentsCreateRequest contract);

        Task<PaymentRequestStatusResponse> GetPaymentRequestStatusAsync(string paymentRequestId, string partnerId);

        Task CancelPaymentRequestAsync(string paymentRequestId, string partnerId);

        Task<PaymentsExecuteResponse> ExecutePaymentRequestAsync(PaymentsExecuteRequest contract);

        Task ProcessPartnersPaymentStatusUpdatedEvent(PartnersPaymentStatusUpdatedEvent statusUpdatedEvent);
    }
}

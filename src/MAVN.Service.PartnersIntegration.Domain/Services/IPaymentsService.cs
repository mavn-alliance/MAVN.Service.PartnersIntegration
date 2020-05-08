using System.Threading.Tasks;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersPayments.Contract;

namespace MAVN.Service.PartnersIntegration.Domain.Services
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

using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PartnersIntegration.Client.Models;
using Refit;

namespace Lykke.Service.PartnersIntegration.Client.Api
{
    /// <summary>
    /// PartnersIntegration Payments client API interface.
    /// </summary>
    [PublicAPI]
    public interface IPaymentsApi
   {
        /// <summary>
        /// Used to create a payment request
        /// </summary>
        /// <param name="model">Information about the payment request</param>
        /// <returns>Information about the creation of the request</returns>
        [Post("/api/payments/requests")]
        Task<PaymentsCreateResponseModel> CreatePaymentRequestAsync([Body]PaymentsCreateRequestModel model);

        /// <summary>
        /// Get the payment request status
        /// </summary>
        /// <param name="paymentRequestId">Id of the payment request</param>
        /// <param name="partnerId">Id of the partner calling the endpoint</param>
        /// <returns>Status information</returns>
        [Get("/api/payments/requests")]
        Task<PaymentRequestStatusResponseModel> GetPaymentRequestStatusAsync([Query]string paymentRequestId, [Query]string partnerId);

        /// <summary>
        /// Cancel the payment request
        /// </summary>
        /// <param name="paymentRequestId">Id of the payment request</param>
        /// <param name="partnerId">Id of the partner calling the endpoint</param>
        [Delete("/api/payments/requests")]
        Task CancelPaymentRequestAsync([Query]string paymentRequestId, [Query]string partnerId);

        /// <summary>
        /// Execute the payment request
        /// </summary>
        /// <param name="model">Model containing information about the execution-</param>
        [Post("/api/payments")]
        Task<PaymentsExecuteResponseModel> ExecutePaymentRequestAsync([Body]PaymentsExecuteRequestModel model);
    }
}

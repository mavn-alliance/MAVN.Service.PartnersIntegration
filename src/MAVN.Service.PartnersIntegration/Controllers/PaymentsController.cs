using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using MAVN.Service.PartnersIntegration.Client.Api;
using MAVN.Service.PartnersIntegration.Client.Models;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace MAVN.Service.PartnersIntegration.Controllers
{
    [ApiController]
    [Route("/api/payments/")]
    public class PaymentsController : Controller, IPaymentsApi
    {
        private readonly IMapper _mapper;
        private readonly IPaymentsService _paymentsService;

        public PaymentsController(IMapper mapper, IPaymentsService paymentsService)
        {
            _mapper = mapper;
            _paymentsService = paymentsService;
        }

        /// <summary>
        /// Used to create a payment request
        /// </summary>
        /// <param name="model">Information about the payment request</param>
        /// <returns>Information about the creation of the request</returns>
        [HttpPost("requests")]
        public async Task<PaymentsCreateResponseModel> CreatePaymentRequestAsync([FromBody]PaymentsCreateRequestModel model)
        {
            var responseContract = await _paymentsService.CreatePaymentRequestAsync(_mapper.Map<PaymentsCreateRequest>(model));

            return _mapper.Map<PaymentsCreateResponseModel>(responseContract);
        }

        /// <summary>
        /// Get the payment request status
        /// </summary>
        /// <param name="paymentRequestId">Id of the payment request</param>
        /// <returns>Status information</returns>
        [HttpGet("requests")]
        public async Task<PaymentRequestStatusResponseModel> GetPaymentRequestStatusAsync([FromQuery][Required]string paymentRequestId,
            [FromQuery][Required]string partnerId)
        {
            var responseContract = await _paymentsService.GetPaymentRequestStatusAsync(paymentRequestId, partnerId);

            return _mapper.Map<PaymentRequestStatusResponseModel>(responseContract);
        }

        /// <summary>
        /// Cancel the payment request
        /// </summary>
        /// <param name="paymentRequestId">Id of the payment request</param>
        [HttpDelete("requests")]
        public async Task CancelPaymentRequestAsync([FromQuery][Required]string paymentRequestId, 
            [FromQuery][Required]string partnerId)
        {
            await _paymentsService.CancelPaymentRequestAsync(paymentRequestId, partnerId);
        }

        /// <summary>
        /// Execute the payment request
        /// </summary>
        /// <param name="paymentRequestId">Id of the payment request</param>
        [HttpPost]
        public async Task<PaymentsExecuteResponseModel> ExecutePaymentRequestAsync([FromBody]PaymentsExecuteRequestModel model)
        {
            var responseContract = await _paymentsService
                .ExecutePaymentRequestAsync(_mapper.Map<PaymentsExecuteRequest>(model));

            return _mapper.Map<PaymentsExecuteResponseModel>(responseContract);
        }
    }
}

using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Falcon.Numerics;
using Lykke.Common.Log;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using Lykke.Service.PartnersIntegration.Domain.Enums;
using Lykke.Service.PartnersIntegration.Domain.Helpers;
using Lykke.Service.PartnersIntegration.Domain.Models;
using Lykke.Service.PartnersIntegration.Domain.Repositories;
using Lykke.Service.PartnersIntegration.Domain.Services;
using Lykke.Service.PartnersPayments.Client;
using Lykke.Service.PartnersPayments.Client.Enums;
using Lykke.Service.PartnersPayments.Client.Models;
using Lykke.Service.PartnersPayments.Contract;
using PaymentRequestStatus = Lykke.Service.PartnersIntegration.Domain.Enums.PaymentRequestStatus;

[assembly: InternalsVisibleTo("Lykke.Service.PartnersIntegration.Tests")]

namespace Lykke.Service.PartnersIntegration.DomainServices.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IMapper _mapper;
        private readonly IPartnerAndLocationHelper _partnerAndLocationHelper;
        private readonly IPartnersPaymentsClient _partnersPaymentsClient;
        private readonly IPartnerManagementClient _partnerManagementClient;
        private readonly IPaymentCallbackRepository _paymentCallbackRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly int _externalPartnerPaymentConnectionRetries;
        private readonly ILog _log;
        private readonly IMessagesService _messagesService;

        public PaymentsService(IMapper mapper,
            ILogFactory logFactory,
            IPartnerAndLocationHelper partnerAndLocationHelper,
            IPartnersPaymentsClient partnersPaymentsClient,
            IPartnerManagementClient partnerManagementClient, 
            IMessagesService messagesService,
            IPaymentCallbackRepository paymentCallbackRepository,
            IHttpClientFactory httpClientFactory,
            int externalPartnerPaymentConnectionRetries)
        {
            _mapper = mapper;
            _partnerAndLocationHelper = partnerAndLocationHelper;
            _partnersPaymentsClient = partnersPaymentsClient;
            _partnerManagementClient = partnerManagementClient;
            _messagesService = messagesService;
            _paymentCallbackRepository = paymentCallbackRepository;
            _httpClientFactory = httpClientFactory;
            _externalPartnerPaymentConnectionRetries = externalPartnerPaymentConnectionRetries;
            _log = logFactory.CreateLog(this);
        }

        public async Task<PaymentsCreateResponse> CreatePaymentRequestAsync(PaymentsCreateRequest contract)
        {
            _log.Info("Creating payment request",
                new {contract.CustomerId});

            ValidateCreatePaymentRequest(contract);

            LocationInfoResponse locationInfoResponse = null;

            var paymentRequestModel = _mapper.Map<PaymentRequestModel>(contract);
            if (!string.IsNullOrWhiteSpace(contract.ExternalLocationId))
            {
                locationInfoResponse = await _partnerManagementClient.Locations.GetByExternalId2Async(contract.ExternalLocationId);

                if (locationInfoResponse == null)
                {
                    return new PaymentsCreateResponse { Status = PaymentCreateStatus.LocationNotFound };
                }

                paymentRequestModel.LocationId = locationInfoResponse.Id.ToString();
            }

            var partnerAndLocationStatus = await _partnerAndLocationHelper.ValidatePartnerInfo(contract.PartnerId, locationInfoResponse);

            if (partnerAndLocationStatus != PartnerAndLocationStatus.OK)
            {
                if (partnerAndLocationStatus == PartnerAndLocationStatus.PartnerNotFound)
                    return new PaymentsCreateResponse { Status = PaymentCreateStatus.PartnerNotFound };
                if (partnerAndLocationStatus == PartnerAndLocationStatus.LocationNotFound)
                    return new PaymentsCreateResponse { Status = PaymentCreateStatus.LocationNotFound };
            }

            if (contract.PaymentInfo != null)
            {
                var sendMessageResponse =
                    await _messagesService.SendMessageAsync(_mapper.Map<MessagesPostRequest>(contract));

                switch (sendMessageResponse.ErrorCode)
                {
                    case MessagesErrorCode.CustomerIsBlocked:
                        return new PaymentsCreateResponse { Status = PaymentCreateStatus.CustomerIsBlocked };

                    case MessagesErrorCode.CustomerNotFound:
                        return new PaymentsCreateResponse { Status = PaymentCreateStatus.CustomerNotFound };

                    case MessagesErrorCode.LocationNotFound:
                        return new PaymentsCreateResponse { Status = PaymentCreateStatus.LocationNotFound };

                    case MessagesErrorCode.PartnerNotFound:
                        return new PaymentsCreateResponse { Status = PaymentCreateStatus.PartnerNotFound };
                }

                paymentRequestModel.PartnerMessageId = sendMessageResponse.PartnerMessageId;
            }

            var responseModel = await _partnersPaymentsClient.Api.PartnerPaymentAsync(paymentRequestModel);

            var responseContract = new PaymentsCreateResponse()
            {
                PaymentRequestId = responseModel.PaymentRequestId, Status = PaymentCreateStatus.OK
            };

            if (responseModel.Error != PaymentRequestErrorCodes.None)
            {
                _log.Error(null, $"Received error code {responseModel.Error}", responseModel.PaymentRequestId);
                responseContract.Status = _mapper.Map<PaymentCreateStatus>(responseModel.Error);
            }

            //Only save if everything is ok and callback url is provided
            if (responseModel.Error == PaymentRequestErrorCodes.None &&
                !string.IsNullOrEmpty(contract.PaymentProcessedCallbackUrl))
            {
                await _paymentCallbackRepository.InsertAsync(new PaymentProcessedCallbackUrl
                {
                    PaymentRequestId = responseModel.PaymentRequestId,
                    RequestAuthToken = contract.RequestAuthToken,
                    Url = contract.PaymentProcessedCallbackUrl
                });
            }

            return responseContract;
        }

        public async Task<PaymentRequestStatusResponse> GetPaymentRequestStatusAsync(string paymentRequestId, string partnerId)
        {
            _log.Info("Getting payment request status information", new {paymentRequestId});

            if (string.IsNullOrWhiteSpace(paymentRequestId))
            {
                throw new ArgumentNullException(nameof(paymentRequestId));
            }

            var responseModel = await _partnersPaymentsClient.Api.GetPaymentDetailsAsync(paymentRequestId);

            if (responseModel == null)
            {
                _log.Warning("Could not find payment request", null, new { paymentRequestId });

                return new PaymentRequestStatusResponse { Status = PaymentRequestStatus.PaymentRequestNotFound };
            }

            if (responseModel.PartnerId != partnerId)
            {
                _log.Warning($"Partner with id {partnerId} is not associated with payment request id {paymentRequestId}",
                    null, new { paymentRequestId, partnerId });
                return new PaymentRequestStatusResponse {Status = PaymentRequestStatus.PaymentRequestNotFound};
            }

            (decimal? fiatAmount, Money18 tokensAmount) = GetPaymentRequestAmounts(responseModel);

            if (fiatAmount == null)
            {
                throw new ArgumentNullException("FiatSendingAmount",
                    $"Payment Request's FiatSendingAmount was null, returned FiatAmount for Payment Request with ID {paymentRequestId}");
            }

            var responseContract = new PaymentRequestStatusResponse
            {
                FiatAmount = fiatAmount.Value,
                FiatCurrency = responseModel.Currency,
                TokensAmount = tokensAmount,
                TotalFiatAmount = responseModel.TotalBillAmount,
                PaymentRequestApprovedTimestamp = responseModel.TokensReserveTimestamp,
                PaymentExecutionTimestamp = responseModel.TokensBurnTimestamp,
                PaymentRequestTimestamp = responseModel.Timestamp,
                Status = ProcessPaymentRequestStatus(responseModel.Status),
                PaymentRequestCustomerExpirationTimestamp = responseModel.CustomerActionExpirationTimestamp
            };

            return responseContract;
        }

        public async Task CancelPaymentRequestAsync(string paymentRequestId, string partnerId)
        {
            _log.Info("Canceling payment request", new {paymentRequestId});

            if (string.IsNullOrWhiteSpace(paymentRequestId))
            {
                throw new ArgumentNullException(nameof(paymentRequestId));
            }
            
            var responseModel = await _partnersPaymentsClient.Api.GetPaymentDetailsAsync(paymentRequestId);

            if (responseModel == null)
            {
                _log.Warning("Could not find payment request", null, new { paymentRequestId });

                return;
            }

            if (responseModel.PartnerId != partnerId)
            {
                _log.Warning($"Partner with id {partnerId} is not associated with payment request id {paymentRequestId}",
                    null, new { paymentRequestId, partnerId });
                return;
            }
            
            await _partnersPaymentsClient.Api.PartnerCancelPaymentAsync(new ReceptionistProcessPaymentRequest
            {
                PaymentRequestId = paymentRequestId
            });
        }

        public async Task<PaymentsExecuteResponse> ExecutePaymentRequestAsync(PaymentsExecuteRequest contract)
        {
            _log.Info("Executing payment request", new { contract.PaymentRequestId });

            if (string.IsNullOrWhiteSpace(contract.PaymentRequestId))
            {
                throw new ArgumentNullException(nameof(contract.PaymentRequestId));
            }

            var responseModel = await _partnersPaymentsClient.Api.GetPaymentDetailsAsync(contract.PaymentRequestId);

            if (responseModel == null)
            {
                _log.Warning("Could not find payment request", null, new { contract.PaymentRequestId });

                return new PaymentsExecuteResponse { Status = PaymentExecuteStatus.PaymentRequestNotFound };
            }

            if (responseModel.PartnerId != contract.PartnerId)
            {
                _log.Warning($"Partner with id {contract.PartnerId} is not associated with payment request id {contract.PaymentRequestId}",
                    null, new { contract.PaymentRequestId, contract.PartnerId });

                return new PaymentsExecuteResponse { Status = PaymentExecuteStatus.PaymentRequestNotFound };
            }

            var response = await _partnersPaymentsClient.Api.ReceptionistApprovePaymentAsync(new ReceptionistProcessPaymentRequest
            {
                PaymentRequestId = contract.PaymentRequestId
            });

            var paymentExecuteStatus = PaymentExecuteStatus.OK;

            if (response.Error != PaymentStatusUpdateErrorCodes.None)
            {
                paymentExecuteStatus = response.Error == PaymentStatusUpdateErrorCodes.PaymentDoesNotExist ?
                    PaymentExecuteStatus.PaymentRequestNotFound :
                    PaymentExecuteStatus.PaymentRequestNotValid;
            }

            if (paymentExecuteStatus == PaymentExecuteStatus.PaymentRequestNotFound)
            {
                return new PaymentsExecuteResponse {Status = paymentExecuteStatus};
            }

            (decimal? fiatAmount, Money18 tokensAmount) = GetPaymentRequestAmounts(responseModel);

            if (fiatAmount == null)
            {
                _log.Error(null, "Payment Request's FiatSendingAmount was null," +
                                 $" returned FiatAmount for Payment Request with ID {contract.PaymentRequestId}"
                    , contract);
                fiatAmount = 0;
            }

            var paymentsExecuteResponseContract = new PaymentsExecuteResponse
            {
                Status = paymentExecuteStatus,
                CustomerId = responseModel.CustomerId,
                TokensAmount = tokensAmount,
                FiatAmount = fiatAmount.Value,
                Currency = responseModel.Currency,
                PaymentId = responseModel.PartnerId,
            };

            return paymentsExecuteResponseContract;
        }

        public async Task ProcessPartnersPaymentStatusUpdatedEvent(PartnersPaymentStatusUpdatedEvent statusUpdatedEvent)
        {
            _log.Info($"Received partner payment status update: {statusUpdatedEvent.Status}",
                new { statusUpdatedEvent.PaymentRequestId, statusUpdatedEvent.Status });

            HttpMethod httpMethod;
            switch (statusUpdatedEvent.Status)
            {
                case PartnerPaymentStatus.TokensTransferSucceeded:
                    httpMethod = HttpMethod.Post;
                    break;
                case PartnerPaymentStatus.RejectedByCustomer:
                case PartnerPaymentStatus.RequestExpired:
                case PartnerPaymentStatus.TokensRefundSucceeded:
                case PartnerPaymentStatus.ExpirationTokensRefundSucceeded:
                case PartnerPaymentStatus.CancelledByPartner:
                    httpMethod = HttpMethod.Delete;
                    break;
                case PartnerPaymentStatus.Created:
                case PartnerPaymentStatus.TokensTransferStarted:
                case PartnerPaymentStatus.TokensBurnSucceeded:
                case PartnerPaymentStatus.ExpirationTokensRefundStarted:
                case PartnerPaymentStatus.TokensRefundStarted:
                case PartnerPaymentStatus.TokensBurnStarted:
                case PartnerPaymentStatus.ExpirationTokensRefundFailed:
                case PartnerPaymentStatus.TokensRefundFailed:
                case PartnerPaymentStatus.TokensBurnFailed:
                case PartnerPaymentStatus.TokensTransferFailed:
                default:
                    return;
            }

            var paymentProcessedCallbackUrl = await _paymentCallbackRepository.GetByIdAsync(statusUpdatedEvent.PaymentRequestId);
            if (paymentProcessedCallbackUrl == null)
            {
                _log.Info(
                    $"PaymentProcessedCallbackUrl not found for PaymentRequestId {statusUpdatedEvent.PaymentRequestId}",
                    new {statusUpdatedEvent.PaymentRequestId, statusUpdatedEvent.Status});
                return;
            }

            var callbackInfo = new
            {
                statusUpdatedEvent.PaymentRequestId,
                httpMethod = httpMethod.ToString(),
                statusUpdatedEvent.Status,
                paymentProcessedCallbackUrl.Url
            };

            try
            {
                var partnersCallbackUrl = new Uri(new Uri(paymentProcessedCallbackUrl.Url),
                    paymentProcessedCallbackUrl.PaymentRequestId).ToString();

                _log.Info($"Calling partner's callback payment service endpoint at url: {httpMethod} {partnersCallbackUrl}",
                    callbackInfo);

                await SendHttpRequestMessage(httpMethod, partnersCallbackUrl,
                    paymentProcessedCallbackUrl.RequestAuthToken, callbackInfo);
            }
            catch (Exception e)
            {
                _log.Error(e, $"Error while calling partner's callback payment service endpoint at url: {httpMethod} {paymentProcessedCallbackUrl.Url}",
                    callbackInfo);
            }
        }
        private (decimal? fiatAmount, Money18 tokensAmount) GetPaymentRequestAmounts(PaymentDetailsResponseModel responseModel)
        {
            Money18 tokensAmount;
            decimal? fiatAmount;

            if (responseModel.TokensSendingAmount.HasValue)
            {
                tokensAmount = responseModel.TokensSendingAmount.Value;
                if (responseModel.FiatSendingAmount.HasValue)
                {
                    fiatAmount = responseModel.FiatSendingAmount.Value;
                }
                else
                {
                    fiatAmount = null;
                }
            }
            else
            {
                tokensAmount = responseModel.TokensAmount;
                fiatAmount = responseModel.FiatAmount;
            }

            return (fiatAmount, tokensAmount);
        }

        private async Task SendHttpRequestMessage(HttpMethod httpMethod, string url, string authToken, object callbackInfo)
        {
            var request = new HttpRequestMessage(httpMethod,
                url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var currentRetry = 0;

            while (currentRetry < _externalPartnerPaymentConnectionRetries)
            {
                try
                {
                    await _httpClientFactory.CreateClient().SendAsync(request);

                    break;
                }
                catch (TimeoutException e)
                {
                    currentRetry++;
                    _log.Warning("Callback connection timed out", e, callbackInfo);
                    if (currentRetry == _externalPartnerPaymentConnectionRetries)
                    {
                        throw;
                    }
                }
            }

            _log.Info("Successfully sent callback to partner", callbackInfo);
        }

        private PaymentRequestStatus ProcessPaymentRequestStatus(PartnersPayments.Client.Enums.PaymentRequestStatus responseModelStatus)
        {
            switch (responseModelStatus)
            {
                case PartnersPayments.Client.Enums.PaymentRequestStatus.Created:
                case PartnersPayments.Client.Enums.PaymentRequestStatus.TokensTransferStarted:
                    return PaymentRequestStatus.PendingCustomerConfirmation;
                case PartnersPayments.Client.Enums.PaymentRequestStatus.RejectedByCustomer:
                    return PaymentRequestStatus.RejectedByCustomer;
                case PartnersPayments.Client.Enums.PaymentRequestStatus.TokensTransferSucceeded:
                case PartnersPayments.Client.Enums.PaymentRequestStatus.TokensBurnStarted:
                case PartnersPayments.Client.Enums.PaymentRequestStatus.TokensRefundStarted:
                    return PaymentRequestStatus.PendingPartnerConfirmation;
                case PartnersPayments.Client.Enums.PaymentRequestStatus.TokensBurnSucceeded:
                    return PaymentRequestStatus.PaymentExecuted;
                case PartnersPayments.Client.Enums.PaymentRequestStatus.TokensRefundSucceeded:
                case PartnersPayments.Client.Enums.PaymentRequestStatus.CancelledByPartner:
                    return PaymentRequestStatus.CancelledByPartner;
                case PartnersPayments.Client.Enums.PaymentRequestStatus.RequestExpired:
                    return PaymentRequestStatus.RequestExpired;
                case PartnersPayments.Client.Enums.PaymentRequestStatus.ExpirationTokensRefundStarted:
                case PartnersPayments.Client.Enums.PaymentRequestStatus.ExpirationTokensRefundSucceeded:
                    return PaymentRequestStatus.PaymentExpired;
                case PartnersPayments.Client.Enums.PaymentRequestStatus.ExpirationTokensRefundFailed:
                case PartnersPayments.Client.Enums.PaymentRequestStatus.TokensRefundFailed:
                case PartnersPayments.Client.Enums.PaymentRequestStatus.TokensTransferFailed:
                case PartnersPayments.Client.Enums.PaymentRequestStatus.TokensBurnFailed:
                default:
                    return PaymentRequestStatus.OperationFailed;
            }
        }

        internal void ValidateCreatePaymentRequest(PaymentsCreateRequest contract)
        {
            if (!contract.FiatAmount.HasValue && !contract.TokensAmount.HasValue)
            {
                throw new ArgumentNullException(nameof(contract.FiatAmount), "Fiat or Token amount required");
            }
            
            if (string.IsNullOrWhiteSpace(contract.CustomerId))
            {
                throw new ArgumentNullException(nameof(contract.CustomerId));
            }

            if (string.IsNullOrWhiteSpace(contract.PartnerId))
            {
                throw new ArgumentNullException(nameof(contract.PartnerId));
            }

            if (string.IsNullOrWhiteSpace(contract.ExternalLocationId))
            {
                throw new ArgumentNullException(nameof(contract.ExternalLocationId));
            }

            if (!contract.TokensAmount.HasValue)
            {
                if (contract.FiatAmount.Value <= 0)
                {
                    throw new ArgumentException("FiatAmount must be a positive number");
                }

                if (string.IsNullOrWhiteSpace(contract.Currency))
                {
                    throw new ArgumentNullException(nameof(contract.Currency));
                }

                if (contract.Currency.Length > 20)
                {
                    throw new ArgumentException($"{nameof(contract.Currency)} must not be bigger than 20 characters");
                }
            }

            if (!contract.FiatAmount.HasValue)
            {
                if (contract.TokensAmount.Value <= 0)
                {
                    throw new ArgumentException("TokensAmount must be a positive number");
                }
            }

            if (contract.ExpirationTimeoutInSeconds.HasValue)
            {
                if (contract.ExpirationTimeoutInSeconds.Value <= 0)
                {
                    throw new ArgumentException("ExpirationTimeoutInSeconds must be a positive number");
                }
            }
        }
    }
}

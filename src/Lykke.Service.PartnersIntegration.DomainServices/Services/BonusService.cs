using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using Lykke.Service.PartnersIntegration.Contract;
using Lykke.Service.PartnersIntegration.Domain.Enums;
using Lykke.Service.PartnersIntegration.Domain.Helpers;
using Lykke.Service.PartnersIntegration.Domain.Models;
using Lykke.Service.PartnersIntegration.Domain.Services;
using Lykke.Service.Referral.Client;
using Lykke.Service.Referral.Client.Enums;
using Lykke.Service.Referral.Client.Models.Requests;

namespace Lykke.Service.PartnersIntegration.DomainServices.Services
{
    public class BonusService : IBonusService
    {
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly IRabbitPublisher<BonusCustomerTriggerEvent> _bonusCustomerEventPublisher;
        private readonly IPartnerAndLocationHelper _partnerAndLocationHelper;
        private readonly IReferralClient _referralClient;
        private readonly IPartnerManagementClient _partnerManagementClient;
        private readonly ILog _log;

        public BonusService(ILogFactory logFactory,
            ICustomerProfileClient customerProfileClient,
            IRabbitPublisher<BonusCustomerTriggerEvent> bonusCustomerEventPublisher,
            IPartnerAndLocationHelper partnerAndLocationHelper,
            IReferralClient referralClient,
            IPartnerManagementClient partnerManagementClient)
        {
            _customerProfileClient = customerProfileClient;
            _bonusCustomerEventPublisher = bonusCustomerEventPublisher;
            _partnerAndLocationHelper = partnerAndLocationHelper;
            _referralClient = referralClient;
            _partnerManagementClient = partnerManagementClient;
            _log = logFactory.CreateLog(this);
        }

        public async Task<List<BonusCustomerResponse>> TriggerBonusToCustomersAsync(List<BonusCustomerRequest> contracts)
        {
            var response = new List<BonusCustomerResponse>();

            for (var seqNumber = 1; seqNumber <= contracts.Count; seqNumber++)
            {
                var contract = contracts[seqNumber - 1];

                BonusCustomerStatus bonusCustomerStatus;

                try
                {
                    bonusCustomerStatus = await ProcessBonusCustomer(contract);
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Error during triggering bonus for customer with id {contract.CustomerId}", new { contract.CustomerId, contract.PartnerId });
                    bonusCustomerStatus = BonusCustomerStatus.TechnicalProblem;
                }

                var bonusCustomerResponse = new BonusCustomerResponse
                {
                    CustomerId = contract.CustomerId,
                    CustomerEmail = contract.Email,
                    BonusCustomerSeqNumber = seqNumber,
                    Status = bonusCustomerStatus
                };

                response.Add(bonusCustomerResponse);
            }

            return response;
        }

        private async Task<BonusCustomerStatus> ProcessBonusCustomer(BonusCustomerRequest contract)
        {
            var emailHash = HashHelper.ComputeSha256Hash(contract.Email);
            _log.Info($"Started processing bonus for customer with id: {contract.CustomerId}",
                new { contract.CustomerId, emailHash, contract.PartnerId, LocationId = contract.ExternalLocationId });

            var bonusCustomerStatus = ValidateBonusCustomerRequestData(contract);

            if (bonusCustomerStatus != BonusCustomerStatus.OK)
            {
                _log.Info($"Stopped processing bonus for customer with id: {contract.CustomerId}" +
                          " due to invalid request data", new { contract.CustomerId, emailHash });
                return bonusCustomerStatus;
            }

            LocationInfoResponse locationInfoResponse = null;

            if (!string.IsNullOrWhiteSpace(contract.ExternalLocationId))
            {
                locationInfoResponse = await _partnerManagementClient.Locations.GetByExternalId2Async(contract.ExternalLocationId);

                if (locationInfoResponse == null)
                {
                    return BonusCustomerStatus.LocationNotFound;
                }
            }

            var partnerInfo = await _partnerAndLocationHelper.ValidatePartnerInfo(contract.PartnerId, locationInfoResponse);

            if (partnerInfo != PartnerAndLocationStatus.OK)
            {
                if (partnerInfo == PartnerAndLocationStatus.PartnerNotFound)
                    return BonusCustomerStatus.PartnerNotFound;
                if (partnerInfo == PartnerAndLocationStatus.LocationNotFound)
                    return BonusCustomerStatus.LocationNotFound;
            }

            var emailMissing = string.IsNullOrWhiteSpace(contract.Email);
            var customerIdMissing = string.IsNullOrWhiteSpace(contract.CustomerId);

            CustomerProfileResponse customer;

            if (!emailMissing && !customerIdMissing)
            {
                customer = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(contract.CustomerId);

                if (customer.Profile == null)
                {
                    _log.Warning($"Customer not found with id: {contract.CustomerId}", null, contract.CustomerId);
                    return BonusCustomerStatus.CustomerNotFound;
                }

                if (customer.Profile.Email != contract.Email)
                {
                    _log.Info($"Stopped processing bonus for customer with id: {contract.CustomerId}" +
                        " due to customerId and email miss-match", new { contract.CustomerId, emailHash });
                    return BonusCustomerStatus.CustomerIdDoesNotMatchEmail;
                }
            }

            if (!customerIdMissing && emailMissing)
            {
                customer = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(contract.CustomerId);

                if (customer.Profile == null)
                {
                    _log.Warning($"Customer not found with id: {contract.CustomerId}", null, contract.CustomerId);
                    return BonusCustomerStatus.CustomerNotFound;
                }

                contract.Email = customer.Profile.Email;
            }
            else if (customerIdMissing && !emailMissing)
            {
                customer = await _customerProfileClient.CustomerProfiles.GetByEmailAsync(
                    new GetByEmailRequestModel { Email = contract.Email });

                if (customer.Profile == null)
                {
                    _log.Warning($"Customer not found with email: {emailHash}", null, emailHash);
                    return BonusCustomerStatus.CustomerNotFound;
                }

                contract.CustomerId = customer.Profile.CustomerId;
            }

            var fiatAmountHash = HashHelper.ComputeSha256Hash(contract.FiatAmount.Value.ToString());

            _log.Info($"Using hotel referral for email {emailHash}", new
            {
                contract.CustomerId,
                contract.PartnerId,
                contract.Currency,
                LocationId = contract.ExternalLocationId,
                contract.PosId,
                contract.PaymentTimestamp,
                fiatAmountHash,
                emailHash
            });

            var useResponse = await _referralClient.ReferralHotelsApi.UseAsync(new ReferralHotelUseRequest
            {
                PartnerId = contract.PartnerId,
                Amount = contract.FiatAmount.Value,
                BuyerEmail = contract.Email,
                CurrencyCode = contract.Currency,
                Location = contract.ExternalLocationId
            });

            if (useResponse.ErrorCode != ReferralHotelUseErrorCode.None)
            {
                var errorCode = useResponse.ErrorCode.ToString();
                _log.Warning($"Could not use referral for customerId: {contract.CustomerId}", null, new { contract.CustomerId, errorCode });
            }

            _log.Info($"Publishing bonus trigger event with CustomerId: {contract.CustomerId}" +
                      $" and PartnerId: {contract.PartnerId}", new
                      {
                          contract.CustomerId,
                          contract.PartnerId,
                          contract.Currency,
                          LocationId = contract.ExternalLocationId,
                          contract.PosId,
                          contract.PaymentTimestamp,
                          fiatAmountHash,
                          emailHash
                      });

            await _bonusCustomerEventPublisher.PublishAsync(new BonusCustomerTriggerEvent
            {
                CustomerId = contract.CustomerId,
                PartnerId = contract.PartnerId,
                Currency = contract.Currency,
                Amount = contract.FiatAmount.Value,
                LocationId = contract.ExternalLocationId
            });

            _log.Info($"Finished processing bonus for customer with id: {contract.CustomerId}",
                new
                {
                    contract.CustomerId,
                    contract.PartnerId,
                    contract.Currency,
                    LocationId = contract.ExternalLocationId,
                    contract.PosId,
                    contract.PaymentTimestamp,
                    fiatAmountHash,
                    emailHash
                });

            return BonusCustomerStatus.OK;
        }

        private static BonusCustomerStatus ValidateBonusCustomerRequestData(BonusCustomerRequest contract)
        {
            if (string.IsNullOrWhiteSpace(contract.Email) && string.IsNullOrWhiteSpace(contract.CustomerId))
            {
                return BonusCustomerStatus.CustomerNotFound;
            }

            if (string.IsNullOrWhiteSpace(contract.PartnerId))
            {
                return BonusCustomerStatus.PartnerNotFound;
            }

            if (!contract.FiatAmount.HasValue)
            {
                return BonusCustomerStatus.InvalidFiatAmount;
            }

            if (string.IsNullOrWhiteSpace(contract.Currency))
            {
                return BonusCustomerStatus.InvalidCurrency;
            }

            if (!contract.PaymentTimestamp.HasValue)
            {
                return BonusCustomerStatus.InvalidPaymentTimestamp;
            }
            
            if ((!string.IsNullOrWhiteSpace(contract.CustomerId) && contract.CustomerId.Length > 100) ||
                (!string.IsNullOrWhiteSpace(contract.Email) && contract.Email.Length > 100) ||
                (contract.Currency.Length > 20) ||
                (contract.PartnerId.Length > 100) ||
                (!string.IsNullOrWhiteSpace(contract.ExternalLocationId) && contract.ExternalLocationId.Length > 100) ||
                (!string.IsNullOrWhiteSpace(contract.PosId) && contract.PosId.Length > 100))
            {
                return BonusCustomerStatus.TechnicalProblem;
            }

            return BonusCustomerStatus.OK;
        }
    }
}

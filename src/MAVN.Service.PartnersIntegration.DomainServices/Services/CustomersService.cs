using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.CustomerProfile.Client.Models.Enums;
using MAVN.Service.CustomerProfile.Client.Models.Requests;
using MAVN.Service.CustomerProfile.Client.Models.Responses;
using MAVN.Service.EligibilityEngine.Client;
using MAVN.Service.EligibilityEngine.Client.Enums;
using MAVN.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using MAVN.Service.PartnerManagement.Client;
using MAVN.Service.PartnerManagement.Client.Models.Location;
using MAVN.Service.PartnersIntegration.Domain.Enums;
using MAVN.Service.PartnersIntegration.Domain.Helpers;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.Domain.Services;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.Tiers.Client;

namespace MAVN.Service.PartnersIntegration.DomainServices.Services
{
    public class CustomersService : ICustomersService
    {
        private const string MVNCurrencyCode = "MVN";
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly IMapper _mapper;
        private readonly IPrivateBlockchainFacadeClient _privateBlockchainFacadeClient;
        private readonly IPartnerAndLocationHelper _partnerAndLocationHelper;
        private readonly ITiersClient _tiersClient;
        private readonly IPartnerManagementClient _partnerManagementClient;
        private readonly IEligibilityEngineClient _eligibilityEngineClient;
        private readonly ILog _log;

        public CustomersService(ICustomerProfileClient customerProfileClient,
            IMapper mapper,
            IPrivateBlockchainFacadeClient privateBlockchainFacadeClient,
            IPartnerAndLocationHelper partnerAndLocationHelper,
            ITiersClient tiersClient,
            ILogFactory logFactory,
            IPartnerManagementClient partnerManagementClient,
            IEligibilityEngineClient eligibilityEngineClient)
        {
            _customerProfileClient = customerProfileClient;
            _mapper = mapper;
            _privateBlockchainFacadeClient = privateBlockchainFacadeClient;
            _partnerAndLocationHelper = partnerAndLocationHelper;
            _tiersClient = tiersClient;
            _partnerManagementClient = partnerManagementClient;
            _eligibilityEngineClient = eligibilityEngineClient;
            _log = logFactory.CreateLog(this);
        }

        public async Task<CustomerInformationResponse> GetCustomerInformationAsync(CustomerInformationRequest contract)
        {
            var response = new CustomerInformationResponse
            {
                Status = CustomerInformationStatus.CustomerNotFound
            };

            var logContext = new
            {
                CustomerId = contract.Id,
                Email = HashHelper.ComputeSha256Hash(contract.Email),
                Phone = HashHelper.ComputeSha256Hash(contract.Phone)
            };

            var customersList = new List<CustomerProfileResponse>();

            if (string.IsNullOrWhiteSpace(contract.Id) && string.IsNullOrWhiteSpace(contract.Email) &&
                string.IsNullOrWhiteSpace(contract.Phone))
            {
                throw new ArgumentException("CustomerId, Email or Phone required");
            }

            if (!string.IsNullOrWhiteSpace(contract.Id))
            {
                if (contract.Id.Length > 100)
                {
                    throw new ArgumentException("Id must not be more than 100 characters");
                }

                var customerById = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(contract.Id);

                if (customerById.Profile == null)
                {
                    _log.Warning("Customer not found", null, logContext);
                    return response;
                }

                customersList.Add(customerById);
            }

            if (!string.IsNullOrWhiteSpace(contract.Email))
            {
                if (contract.Email.Length > 100)
                {
                    throw new ArgumentException("Email must not be more than 100 characters");
                }

                var customerByEmail = await _customerProfileClient.CustomerProfiles.GetByEmailAsync(
                    new GetByEmailRequestModel { Email = contract.Email });

                if (customerByEmail.Profile == null)
                {
                    _log.Warning("Customer not found", null, logContext);
                    return response;
                }

                customersList.Add(customerByEmail);
            }

            if (!string.IsNullOrWhiteSpace(contract.Phone))
            {
                if (contract.Phone.Length > 50)
                {
                    throw new ArgumentException("Phone must not be more than 50 characters");
                }

                var customerByPhone = await _customerProfileClient.CustomerProfiles.GetByPhoneAsync(
                    new GetByPhoneRequestModel { Phone = contract.Phone });

                if (customerByPhone.Profile == null)
                {
                    _log.Warning("Customer not found", null, logContext);
                    return response;
                }

                customersList.Add(customerByPhone);
            }

            //Check to see if all customers are the same customer (by profile)
            if (!customersList.All(x => string.Equals(x.Profile.CustomerId, customersList[0].Profile.CustomerId)))
            {
                _log.Warning("Customer not found", null, logContext);
                return response;
            }

            var customer = customersList[0];
            
            response.CustomerId = customer.Profile.CustomerId;
            response.FirstName = customer.Profile.FirstName;
            response.LastName = customer.Profile.LastName;
            response.Status = CustomerInformationStatus.OK;

            var tierLevel = "Black";
            _log.Info("Tier functionality currently not implemented. Returning Black");

            //TODO: Uncomment this once tiering is needed again.
            //if (response.Status == CustomerInformationStatus.OK)
            //{
            //    if (!Guid.TryParse(response.CustomerId, out var customerIdGuid))
            //    {
            //        _log.Warning($"Invalid guid id for customer with id: {response.CustomerId}", null, new { response.CustomerId });
            //        response.Status = CustomerInformationStatus.CustomerNotFound;
            //        return response;
            //    }

            //    var tierResponse = await _tiersClient.Customers.GetTierAsync(customerIdGuid);

            //    if (tierResponse == null)
            //    {
            //        _log.Warning($"No tier found for customer with id {response.CustomerId}", null, new { response.CustomerId });
            //    }
            //    else
            //    {
            //        tierLevel = tierResponse.Name;
            //    }
            //}

            response.TierLevel = tierLevel;
            return response;
        }

        public async Task<CustomerBalanceResponse> GetCustomerBalanceAsync(string customerId,
            CustomerBalanceRequest contract)
        {
            ValidateCustomerBalanceRequestData(customerId, contract);

            var response = new CustomerBalanceResponse
            {
                Status = CustomerBalanceStatus.OK
            };

            LocationInfoResponse locationInfoResponse = null;

            if (!string.IsNullOrWhiteSpace(contract.ExternalLocationId))
            {
                locationInfoResponse = await _partnerManagementClient.Locations.GetByExternalId2Async(contract.ExternalLocationId);

                if (locationInfoResponse == null)
                {
                    response.Status = CustomerBalanceStatus.LocationNotFound;
                    return response;
                }
            }

            var partnerInfo = await _partnerAndLocationHelper.GetPartnerInfo(contract.PartnerId,
                locationInfoResponse);

            if (partnerInfo.PartnerAndLocationStatus != PartnerAndLocationStatus.OK)
            {
                if (partnerInfo.PartnerAndLocationStatus == PartnerAndLocationStatus.PartnerNotFound)
                    response.Status = CustomerBalanceStatus.PartnerNotFound;
                if (partnerInfo.PartnerAndLocationStatus == PartnerAndLocationStatus.LocationNotFound)
                    response.Status = CustomerBalanceStatus.LocationNotFound;
                
                return response;
            }

            var customer = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(customerId);
            
            if (customer.ErrorCode == CustomerProfileErrorCodes.CustomerProfileDoesNotExist)
            {
                _log.Warning($"Customer not found with id: {customerId}", null, customerId);
                response.Status = CustomerBalanceStatus.CustomerNotFound;
                return response;
            }

            var isValid = Guid.TryParse(customerId, out var customerIdGuid);

            if (!isValid)
            {
                _log.Warning($"Invalid customer guid id: {customerId}", null, customerId);
                response.Status = CustomerBalanceStatus.CustomerNotFound;
                return response;
            }

            var customerBalance = await _privateBlockchainFacadeClient.CustomersApi.GetBalanceAsync(customerIdGuid);

            if (customerBalance.Error != CustomerBalanceError.None)
            {
                _log.Error(null, "Could not get customer balance", customerIdGuid);
                response.Status = CustomerBalanceStatus.CustomerNotFound;
                return response;
            }

            response.Tokens = customerBalance.Total;

            var optimalCurrencyRateByPartnerRequest = new ConvertOptimalByPartnerRequest
            {
                CustomerId = customerIdGuid,
                PartnerId = Guid.Parse(contract.PartnerId),
                FromCurrency = MVNCurrencyCode,
                ToCurrency = contract.Currency,
                Amount = Falcon.Numerics.Money18.Parse(customerBalance.Total.ToString())
            };

            var conversionResponse =
                await _eligibilityEngineClient.ConversionRate.ConvertOptimalByPartnerAsync(
                    optimalCurrencyRateByPartnerRequest);

            if (conversionResponse.ErrorCode != EligibilityEngineErrors.None)
            {
                _log.Error(null, "Could not get currency rate", optimalCurrencyRateByPartnerRequest);
                response.Status = _mapper.Map<CustomerBalanceStatus>(conversionResponse.ErrorCode);
                return response;
            }

            response.FiatBalance = decimal.Parse(conversionResponse.Amount.ToString(),
                CultureInfo.InvariantCulture);
            response.FiatCurrency = contract.Currency;

            return response;
        }

        private void ValidateCustomerBalanceRequestData(string customerId, CustomerBalanceRequest contract)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException(nameof(customerId));
            }
            if (customerId.Length > 100)
            {
                throw new ArgumentException($"{nameof(customerId)} must be less than 100 characters");
            }

            if (string.IsNullOrWhiteSpace(contract.PartnerId))
            {
                throw new ArgumentNullException(nameof(contract.PartnerId));
            }
            if (contract.PartnerId.Length > 100)
            {
                throw new ArgumentException($"{nameof(contract.PartnerId)} must be less than 100 characters");
            }

            if (string.IsNullOrWhiteSpace(contract.Currency))
            {
                throw new ArgumentNullException(nameof(contract.Currency));
            }
            if (contract.Currency.Length > 20)
            {
                throw new ArgumentException($"{nameof(contract.Currency)} must be less than 20 characters");
            }
            
            if (!string.IsNullOrWhiteSpace(contract.ExternalLocationId) && contract.ExternalLocationId.Length > 100)
            {
                throw new ArgumentException($"{nameof(contract.ExternalLocationId)} must be less than 100 characters");
            }
        }
    }
}

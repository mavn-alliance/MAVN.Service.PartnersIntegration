using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using MAVN.Service.PartnersIntegration.Domain.Enums;
using MAVN.Service.PartnersIntegration.Domain.Helpers;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.Domain.Services;
using Lykke.Service.Referral.Client;
using Lykke.Service.Referral.Client.Enums;
using Lykke.Service.Referral.Client.Models.Requests;
using Lykke.Service.Referral.Client.Models.Responses;

namespace MAVN.Service.PartnersIntegration.DomainServices.Services
{
    public class ReferralsService : IReferralsService
    {
        private readonly IMapper _mapper;
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly IPartnerAndLocationHelper _partnerAndLocationHelper;
        private readonly ILog _log;
        private readonly IReferralClient _referralClient;
        private readonly IPartnerManagementClient _partnerManagementClient;

        public ReferralsService(ILogFactory logFactory,
            IMapper mapper,
            ICustomerProfileClient customerProfileClient,
            IPartnerAndLocationHelper partnerAndLocationHelper,
            IReferralClient referralClient,
            IPartnerManagementClient partnerManagementClient)
        {
            _mapper = mapper;
            _customerProfileClient = customerProfileClient;
            _partnerAndLocationHelper = partnerAndLocationHelper;
            _referralClient = referralClient;
            _partnerManagementClient = partnerManagementClient;
            _log = logFactory.CreateLog(this);
        }

        public  async Task<ReferralInformationResponse> GetReferralInformationAsync(ReferralInformationRequest contract)
        {
            ValidateReferralInformationRequestData(contract);

            var response = new ReferralInformationResponse
            {
                Status = ReferralInformationStatus.OK,
                Referrals = new List<Domain.Models.Referral>()
            };

            LocationInfoResponse locationInfoResponse = null;

            if (!string.IsNullOrWhiteSpace(contract.ExternalLocationId))
            {
                locationInfoResponse = await _partnerManagementClient.Locations.GetByExternalId2Async(contract.ExternalLocationId);

                if (locationInfoResponse == null)
                {
                    response.Status = ReferralInformationStatus.LocationNotFound;
                    return response;
                }
            }

            var partnerAndLocationStatus = await _partnerAndLocationHelper.ValidatePartnerInfo(contract.PartnerId,
                locationInfoResponse);

            if (partnerAndLocationStatus != PartnerAndLocationStatus.OK)
            {
                if (partnerAndLocationStatus == PartnerAndLocationStatus.PartnerNotFound)
                    response.Status = ReferralInformationStatus.PartnerNotFound;
                if (partnerAndLocationStatus == PartnerAndLocationStatus.LocationNotFound)
                    response.Status = ReferralInformationStatus.LocationNotFound;

                return response;
            }

            var customerProfile = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(contract.CustomerId);

            if (customerProfile.ErrorCode != CustomerProfileErrorCodes.None)
            {
                _log.Warning("Could not find customer profile", null,
                    new {contract.CustomerId, customerProfile.ErrorCode});

                response.Status = ReferralInformationStatus.CustomerNotFound;
                return response;
            }

            var referralInfo =
                await _referralClient.ReferralHotelsApi.GetByEmailAsync(
                    new GetHotelReferralsByEmailRequestModel
                    {
                        Email = customerProfile.Profile.Email,
                        PartnerId = contract.PartnerId,
                        Location = locationInfoResponse?.Id.ToString(),
                    });

            foreach (var hotelReferral in referralInfo.HotelReferrals)
            {
                var referralContract = await GetProcessedReferral(hotelReferral);

                if (referralContract == null)
                {
                    _log.Warning($"Skipped processing referral with Id {hotelReferral.Id} due to technical problem");
                    continue;
                }

                response.Referrals.Add(referralContract);
            }

            return response;
        }

        private async Task<Domain.Models.Referral> GetProcessedReferral(ReferralHotelModel hotelReferral)
        {
            var referralContract = new Domain.Models.Referral
            {
                ReferralId = hotelReferral.Id
            };

            switch (hotelReferral.State)
            {
                case ReferralHotelState.Pending:
                    referralContract.ReferralStatus = ReferralStatus.ReferralNotConfirmed;
                    break;
                case ReferralHotelState.Confirmed:
                    referralContract.ReferralStatus = ReferralStatus.OK;
                    break;
                case ReferralHotelState.Used:
                    referralContract.ReferralStatus = ReferralStatus.ReferralAlreadyUsed;
                    break;
                case ReferralHotelState.Expired:
                    referralContract.ReferralStatus = ReferralStatus.ReferralExpired;
                    break;
                default:
                    _log.Warning($"Received unknown status for hotel referral with id: {hotelReferral.Id}. ",
                        null, hotelReferral.Id);
                    return null;
            }

            var referrer = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(hotelReferral.ReferrerId);

            if (referrer.Profile != null)
            {
                referralContract.ReferrerEmail = referrer.Profile.Email;
                referralContract.ReferrerAdditionalInfo =
                    $"{referrer.Profile.FirstName} {referrer.Profile.LastName}";
            }
            else
            {
                _log.Warning($"Could not get information about referrer with id: {hotelReferral.ReferrerId}." +
                             $" Error code: {referrer.ErrorCode.ToString()}");
            }

            return referralContract;
        }

        private static void ValidateReferralInformationRequestData(ReferralInformationRequest contract)
        {
            if (string.IsNullOrWhiteSpace(contract.CustomerId))
            {
                throw new ArgumentException("CustomerId required");
            }
            if (contract.CustomerId.Length > 100)
            {
                throw new ArgumentException($"{nameof(contract.CustomerId)} must be less than 100 characters");
            }

            if (string.IsNullOrWhiteSpace(contract.PartnerId))
            {
                throw new ArgumentException("Partner Id required");
            }
            if (contract.PartnerId.Length > 100)
            {
                throw new ArgumentException($"{nameof(contract.PartnerId)} must be less than 100 characters");
            }
            
            if (!string.IsNullOrWhiteSpace(contract.ExternalLocationId) && contract.ExternalLocationId.Length > 100)
            {
                throw new ArgumentException($"{nameof(contract.ExternalLocationId)} must be less than 100 characters");
            }
        }
    }
}

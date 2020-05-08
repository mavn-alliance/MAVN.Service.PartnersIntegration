using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.PartnerManagement.Client;
using MAVN.Service.PartnerManagement.Client.Models.Location;
using MAVN.Service.PartnersIntegration.Domain.Enums;
using MAVN.Service.PartnersIntegration.Domain.Helpers;
using MAVN.Service.PartnersIntegration.Domain.Models;

namespace MAVN.Service.PartnersIntegration.DomainServices
{
    public class PartnerAndLocationHelper : IPartnerAndLocationHelper
    {
        private readonly IPartnerManagementClient _partnerManagementClient;
        private readonly ILog _log;

        public PartnerAndLocationHelper(ILogFactory logFactory,
            IPartnerManagementClient partnerManagementClient)
        {
            _partnerManagementClient = partnerManagementClient;
            _log = logFactory.CreateLog(this);
        }

        public async Task<PartnerAndLocationStatus> ValidatePartnerInfo(string partnerId, LocationInfoResponse locationInfoResponse)
        {
            var partnerInfo = await GetPartnerInfo(partnerId, locationInfoResponse);

            return partnerInfo.PartnerAndLocationStatus;
        }

        public async Task<PartnerInfo> GetPartnerInfo(string partnerId, LocationInfoResponse locationInfoResponse)
        {
            var partnerInfoModel = new PartnerInfo
            {
                Id = null,
                Name = null
            };

            if (!Guid.TryParse(partnerId, out var partnerIdGuid))
            {
                _log.Warning($"Invalid partner guid id: {partnerId}", null, new { partnerId });

                partnerInfoModel.PartnerAndLocationStatus = PartnerAndLocationStatus.PartnerNotFound;

                return partnerInfoModel;
            }

            var partner = await _partnerManagementClient.Partners.GetByIdAsync(partnerIdGuid);

            if (partner == null)
            {
                _log.Warning($"Partner not found with id: {partnerId}", null, new { partnerId });

                partnerInfoModel.PartnerAndLocationStatus = PartnerAndLocationStatus.PartnerNotFound;

                return partnerInfoModel;
            }

            if (locationInfoResponse != null && locationInfoResponse.PartnerId != partnerIdGuid)
            {
                _log.Warning($"Location not found with id: {locationInfoResponse.ExternalId}", null, new { locationInfoResponse.ExternalId });

                partnerInfoModel.PartnerAndLocationStatus = PartnerAndLocationStatus.LocationNotFound;

                return partnerInfoModel;
            }
            
            partnerInfoModel.Id = partner.Id;
            partnerInfoModel.Name = partner.Name;
            partnerInfoModel.PartnerAndLocationStatus = PartnerAndLocationStatus.OK;

            return partnerInfoModel;
        }
    }
}

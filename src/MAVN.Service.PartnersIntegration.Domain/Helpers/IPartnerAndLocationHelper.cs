using System.Threading.Tasks;
using MAVN.Service.PartnerManagement.Client.Models.Location;
using MAVN.Service.PartnersIntegration.Domain.Enums;
using MAVN.Service.PartnersIntegration.Domain.Models;

namespace MAVN.Service.PartnersIntegration.Domain.Helpers
{
    public interface IPartnerAndLocationHelper
    {
        Task<PartnerAndLocationStatus> ValidatePartnerInfo(string partnerId, LocationInfoResponse locationId);

        Task<PartnerInfo> GetPartnerInfo(string partnerId, LocationInfoResponse locationInfoResponse);
    }
}

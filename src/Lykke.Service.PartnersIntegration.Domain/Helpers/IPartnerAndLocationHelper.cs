using System.Threading.Tasks;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using Lykke.Service.PartnersIntegration.Domain.Enums;
using Lykke.Service.PartnersIntegration.Domain.Models;

namespace Lykke.Service.PartnersIntegration.Domain.Helpers
{
    public interface IPartnerAndLocationHelper
    {
        Task<PartnerAndLocationStatus> ValidatePartnerInfo(string partnerId, LocationInfoResponse locationId);

        Task<PartnerInfo> GetPartnerInfo(string partnerId, LocationInfoResponse locationInfoResponse);
    }
}

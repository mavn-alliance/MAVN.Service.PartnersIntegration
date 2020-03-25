using System;
using Lykke.Service.PartnersIntegration.Domain.Enums;

namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class PartnerInfo
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public PartnerAndLocationStatus PartnerAndLocationStatus { get; set; }
    }
}

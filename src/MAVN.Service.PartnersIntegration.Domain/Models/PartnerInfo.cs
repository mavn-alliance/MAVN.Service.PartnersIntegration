using System;
using MAVN.Service.PartnersIntegration.Domain.Enums;

namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class PartnerInfo
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public PartnerAndLocationStatus PartnerAndLocationStatus { get; set; }
    }
}

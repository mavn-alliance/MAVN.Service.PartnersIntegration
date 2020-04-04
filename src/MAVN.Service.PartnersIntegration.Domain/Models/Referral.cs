using MAVN.Service.PartnersIntegration.Domain.Enums;

namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class Referral
    {
        public ReferralStatus ReferralStatus { get; set; }

        public string ReferralId { get; set; }

        public string ReferrerEmail { get; set; }

        public string ReferrerAdditionalInfo { get; set; }
    }
}

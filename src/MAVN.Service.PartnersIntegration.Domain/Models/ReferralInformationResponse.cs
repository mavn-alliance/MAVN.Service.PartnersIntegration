using System.Collections.Generic;
using MAVN.Service.PartnersIntegration.Domain.Enums;

namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class ReferralInformationResponse
    {
        public ReferralInformationStatus Status { get; set; }

        public List<Referral> Referrals { get; set; }
    }
}

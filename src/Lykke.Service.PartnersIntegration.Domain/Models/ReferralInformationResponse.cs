using System.Collections.Generic;
using Lykke.Service.PartnersIntegration.Domain.Enums;

namespace Lykke.Service.PartnersIntegration.Domain.Models
{
    public class ReferralInformationResponse
    {
        public ReferralInformationStatus Status { get; set; }

        public List<Referral> Referrals { get; set; }
    }
}

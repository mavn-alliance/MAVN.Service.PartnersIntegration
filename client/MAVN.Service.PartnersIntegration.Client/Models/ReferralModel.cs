using MAVN.Service.PartnersIntegration.Client.Enums;

namespace MAVN.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Model for a single referral response
    /// </summary>
    public class ReferralModel
    {
        /// <summary>
        /// The status of the referral
        /// </summary>
        public ReferralStatus ReferralStatus { get; set; }

        /// <summary>
        /// The id of the referral
        /// </summary>
        public string ReferralId { get; set; }

        /// <summary>
        /// The referrer email
        /// </summary>
        public string ReferrerEmail { get; set; }

        /// <summary>
        /// Referrer additional info
        /// </summary>
        public string ReferrerAdditionalInfo { get; set; }
    }
}

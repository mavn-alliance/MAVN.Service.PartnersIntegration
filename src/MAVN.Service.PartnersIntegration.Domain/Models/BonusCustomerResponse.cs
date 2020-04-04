using MAVN.Service.PartnersIntegration.Domain.Enums;

namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class BonusCustomerResponse
    {
        public BonusCustomerStatus Status { get; set; }

        public string CustomerId { get; set; }

        public string CustomerEmail { get; set; }

        public int BonusCustomerSeqNumber { get; set; }
    }
}

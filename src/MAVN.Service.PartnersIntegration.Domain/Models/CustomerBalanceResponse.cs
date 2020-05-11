using MAVN.Numerics;
using MAVN.Service.PartnersIntegration.Domain.Enums;

namespace MAVN.Service.PartnersIntegration.Domain.Models
{
    public class CustomerBalanceResponse
    {
        public CustomerBalanceStatus Status { get; set; }

        public Money18 Tokens { get; set; }

        public decimal FiatBalance { get; set; }

        public string FiatCurrency { get; set; }
    }
}

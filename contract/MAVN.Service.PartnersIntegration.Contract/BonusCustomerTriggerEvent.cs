namespace MAVN.Service.PartnersIntegration.Contract
{
    public class BonusCustomerTriggerEvent
    {
        public string CustomerId { get; set; }

        public string PartnerId { get; set; }

        public string LocationId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }
    }
}

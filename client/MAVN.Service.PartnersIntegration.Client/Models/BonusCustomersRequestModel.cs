using System;
using System.Collections.Generic;

namespace MAVN.Service.PartnersIntegration.Client.Models
{
    /// <summary>
    /// Model used to request a customer bonus trigger
    /// </summary>
    public class BonusCustomersRequestModel
    {
        /// <summary>
        /// List of trigger requests
        /// </summary>
        public List<BonusCustomerModel> BonusCustomers { get; set; }
    }
}

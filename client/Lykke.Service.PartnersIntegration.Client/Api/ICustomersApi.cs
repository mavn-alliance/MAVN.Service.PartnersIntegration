using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PartnersIntegration.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Lykke.Service.PartnersIntegration.Client.Api
{
    /// <summary>
    /// PartnersIntegration Customer client API interface.
    /// </summary>
    [PublicAPI]
    public interface ICustomersApi
    {
        /// <summary>
        /// Endpoint providing ways to find information about a customer
        /// </summary>
        /// <param name="model">Model containing at least 1 type of identifier on which to find the customer</param>
        /// <returns>Customer information</returns>
        [Post("/api/customers/query")]
        Task<CustomerInformationResponseModel> CustomerInformation(CustomerInformationRequestModel model);

        /// <summary>
        /// Endpoint providing ways to find a customer's balance
        /// </summary>
        /// <param name="customerId">The id of the customer</param>
        /// <param name="model">Model containing information on who is requesting info about the customer</param>
        /// <returns>Customer information</returns>
        [Get("/api/customers/balance/{customerId}")]
        Task<CustomerBalanceResponseModel> GetCustomerBalance(string customerId, [FromQuery] CustomerBalanceRequestModel model);
    }
}

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.PartnersIntegration.Client.Api;
using Lykke.Service.PartnersIntegration.Client.Models;
using Lykke.Service.PartnersIntegration.Domain.Models;
using Lykke.Service.PartnersIntegration.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PartnersIntegration.Controllers
{
    [ApiController]
    [Route("/api/customers/")]
    public class CustomersController : Controller, ICustomersApi
    {
        private readonly IMapper _mapper;
        private readonly ICustomersService _customersService;

        public CustomersController(IMapper mapper, ICustomersService customersService)
        {
            _mapper = mapper;
            _customersService = customersService;
        }

        /// <summary>
        /// Endpoint providing ways to find information about a customer
        /// </summary>
        /// <param name="model">Model containing at least 1 type of identifier on which to find the customer</param>
        /// <returns>Customer information</returns>
        [HttpPost("query")]
        public async Task<CustomerInformationResponseModel> CustomerInformation(CustomerInformationRequestModel model)
        {
            var responseContract = await _customersService.GetCustomerInformationAsync(_mapper.Map<CustomerInformationRequest>(model));

            return _mapper.Map<CustomerInformationResponseModel>(responseContract);
        }

        /// <summary>
        /// Endpoint providing ways to find information about a customer
        /// </summary>
        /// <param name="customerId">The id of the customer</param>
        /// <param name="model">Model containing at least 1 type of identifier on which to find the customer</param>
        /// <returns>Customer information</returns>
        [HttpGet("balance/{customerId}")]
        public async Task<CustomerBalanceResponseModel> GetCustomerBalance(string customerId, 
            [FromQuery] CustomerBalanceRequestModel model)
        {
            var responseContract = await _customersService.GetCustomerBalanceAsync(customerId, _mapper.Map<CustomerBalanceRequest>(model));

            return _mapper.Map<CustomerBalanceResponseModel>(responseContract);
        }
    }
}

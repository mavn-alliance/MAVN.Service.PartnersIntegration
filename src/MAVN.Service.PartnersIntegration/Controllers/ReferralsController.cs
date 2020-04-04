using System.Threading.Tasks;
using AutoMapper;
using MAVN.Service.PartnersIntegration.Client.Api;
using MAVN.Service.PartnersIntegration.Client.Models;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace MAVN.Service.PartnersIntegration.Controllers
{
    [ApiController]
    [Route("/api/referrals/")]
    public class ReferralsController : Controller, IReferralsApi
    {
        private readonly IMapper _mapper;
        private readonly IReferralsService _referralsService;

        public ReferralsController(IMapper mapper, IReferralsService referralsService)
        {
            _mapper = mapper;
            _referralsService = referralsService;
        }

        /// <summary>
        /// Endpoint for retrieving referral information
        /// </summary>
        /// <param name="model">Filter request data</param>
        /// <returns></returns>
        [HttpPost("query")]
        public async Task<ReferralInformationResponseModel> ReferralInformation(ReferralInformationRequestModel model)
        {
            var responseContract = await _referralsService.GetReferralInformationAsync(_mapper.Map<ReferralInformationRequest>(model));

            return _mapper.Map<ReferralInformationResponseModel>(responseContract);
        }
    }
}

using System;
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
    [Route("/api/messages/")]
    public class MessagesController : Controller, IMessagesApi
    {
        private readonly IMapper _mapper;
        private readonly IMessagesService _messagesService;

        public MessagesController(IMapper mapper, IMessagesService messagesService)
        {
            _mapper = mapper;
            _messagesService = messagesService;
        }

        /// <summary>
        /// Endpoint providing a way to send a message from a partner to a customer
        /// </summary>
        /// <param name="model">Contains information on the message to be sent</param>
        /// <returns>Message sent info</returns>
        [HttpPost]
        public async Task<MessagesPostResponseModel> SendMessageAsync(MessagesPostRequestModel model)
        {
            var responseContract = await _messagesService.SendMessageAsync(_mapper.Map<MessagesPostRequest>(model));

            return _mapper.Map<MessagesPostResponseModel>(responseContract);
        }

        /// <summary>
        /// Endpoint providing a way to retrieve message information
        /// </summary>
        /// <param name="partnerMessageId">The id of the message</param>
        /// <returns>Message info</returns>
        [HttpGet("{partnerMessageId}")]
        public async Task<MessageGetResponseModel> GetMessageAsync(string partnerMessageId)
        {
            var responseContract = await _messagesService.GetMessageAsync(partnerMessageId);

            return _mapper.Map<MessageGetResponseModel>(responseContract);
        }

        /// <summary>
        /// Endpoint providing a way to delete message
        /// </summary>
        /// <param name="partnerMessageId">The id of the message</param>
        /// <returns></returns>
        [HttpDelete("{partnerMessageId}")]
        public async Task DeleteMessageAsync(string partnerMessageId)
        {
            await _messagesService.DeleteMessageAsync(partnerMessageId);
        }
    }
}

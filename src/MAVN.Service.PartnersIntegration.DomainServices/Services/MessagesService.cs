using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.CustomerManagement.Client;
using MAVN.Service.CustomerManagement.Client.Enums;
using MAVN.Service.NotificationSystem.SubscriberContract;
using MAVN.Service.PartnerManagement.Client;
using MAVN.Service.PartnerManagement.Client.Models.Location;
using MAVN.Service.PartnersIntegration.Domain.Enums;
using MAVN.Service.PartnersIntegration.Domain.Helpers;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.Domain.Repositories;
using MAVN.Service.PartnersIntegration.Domain.Services;

namespace MAVN.Service.PartnersIntegration.DomainServices.Services
{
    public class MessagesService : IMessagesService
    {
        private readonly IMessagesRepository _messagesRepository;
        private readonly IMessageContentRepository _messageContentRepository;
        private readonly ILog _log;
        private readonly IPartnerAndLocationHelper _partnerAndLocationHelper;
        private readonly ICustomerManagementServiceClient _customerManagementServiceClient;
        private readonly IRabbitPublisher<PushNotificationEvent> _partnerMessagesPublisher;
        private readonly IPartnerManagementClient _partnerManagementClient;
        private readonly string _partnerMessageTemplateId;
        private readonly string _componentSourceName;

        public MessagesService(
            IMessagesRepository messagesRepository,
            IMessageContentRepository messageContentRepository,
            IPartnerAndLocationHelper partnerAndLocationHelper,
            ICustomerManagementServiceClient customerManagementServiceClient,
            IRabbitPublisher<PushNotificationEvent> partnerMessagesPublisher,
            IPartnerManagementClient partnerManagementClient,
            string partnerMessageTemplateId,
            ILogFactory logFactory)
        {
            _messagesRepository = messagesRepository;
            _messageContentRepository = messageContentRepository;
            _partnerAndLocationHelper = partnerAndLocationHelper;
            _customerManagementServiceClient = customerManagementServiceClient;
            _partnerMessagesPublisher = partnerMessagesPublisher;
            _partnerManagementClient = partnerManagementClient;
            _partnerMessageTemplateId = partnerMessageTemplateId;
            _log = logFactory.CreateLog(this);
            _componentSourceName = $"{AppEnvironment.Name} - {AppEnvironment.Version}";
        }

        public async Task<MessagesPostResponse> SendMessageAsync(MessagesPostRequest contract)
        {
            _log.Info("Send Message Async started",
                new
                {
                    contract.CustomerId,
                    LocationId = contract.ExternalLocationId,
                    contract.PartnerId,
                    contract.SendPushNotification,
                    contract.PosId,
                    contract.Subject
                });

            //Check customer id
            var customerBlockState =
                await _customerManagementServiceClient.CustomersApi.GetCustomerBlockStateAsync(contract.CustomerId);

            if (customerBlockState.Error == CustomerBlockStatusError.CustomerNotFound)
            {
                _log.Warning("Customer Not Found", null,
                    new {contract.CustomerId, contract.PartnerId, contract.ExternalLocationId});

                return new MessagesPostResponse
                {
                    PartnerMessageId = null, ErrorCode = MessagesErrorCode.CustomerNotFound
                };
            }
            else if (customerBlockState.Status == CustomerActivityStatus.Blocked)
            {
                _log.Warning("Customer Is Blocked", null,
                    new {contract.CustomerId, contract.PartnerId, contract.ExternalLocationId});

                return new MessagesPostResponse
                {
                    PartnerMessageId = null, ErrorCode = MessagesErrorCode.CustomerIsBlocked
                };
            }

            //Check location
            LocationInfoResponse locationInfoResponse = null;

            if (!string.IsNullOrWhiteSpace(contract.ExternalLocationId))
            {
                locationInfoResponse =
                    await _partnerManagementClient.Locations.GetByExternalId2Async(contract.ExternalLocationId);

                if (locationInfoResponse == null)
                {
                    _log.Warning("Location Not Found", null,
                        new {contract.CustomerId, contract.PartnerId, contract.ExternalLocationId});

                    return new MessagesPostResponse
                    {
                        PartnerMessageId = null, ErrorCode = MessagesErrorCode.LocationNotFound
                    };
                }
            }

            //Check partner and location id
            var partnerAndLocationStatus =
                await _partnerAndLocationHelper.ValidatePartnerInfo(contract.PartnerId, locationInfoResponse);

            if (partnerAndLocationStatus != PartnerAndLocationStatus.OK)
            {
                var (message, errorCode) = partnerAndLocationStatus == PartnerAndLocationStatus.LocationNotFound
                    ? ("Location Not Found", MessagesErrorCode.LocationNotFound)
                    : ("Partner Not Found", MessagesErrorCode.PartnerNotFound);

                _log.Warning(message, null, new {contract.CustomerId, contract.PartnerId, contract.ExternalLocationId});

                return new MessagesPostResponse {PartnerMessageId = null, ErrorCode = errorCode};
            }

            var messageId = await _messagesRepository.InsertAsync(contract);

            _log.Info("Message saved", messageId);

            try
            {
                await _messageContentRepository.SaveContentAsync(messageId.ToString(), contract.Message);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to save message content", new {messageId, contract.Message});

                await _messagesRepository.DeleteMessageAsync(messageId.ToString());

                throw;
            }

            _log.Info("Message content saved", messageId);

            //Send push notification via Rabbit
            if (contract.SendPushNotification.HasValue && contract.SendPushNotification.Value)
            {
                var evt = new PushNotificationEvent
                {
                    CustomerId = contract.CustomerId,
                    MessageTemplateId = _partnerMessageTemplateId,
                    Source = _componentSourceName,
                    TemplateParameters =
                        new Dictionary<string, string>
                        {
                            {"Subject", contract.Subject},
                            {"PartnerMessageId", messageId.ToString()}
                        }
                };

                await _partnerMessagesPublisher.PublishAsync(evt);

                _log.Info("Partner message published", new {messageId, contract.CustomerId, contract.Subject});
            }

            return new MessagesPostResponse
            {
                ErrorCode = MessagesErrorCode.OK, PartnerMessageId = messageId.ToString()
            };
        }

        public async Task<MessageGetResponse> GetMessageAsync(string partnerMessageId)
        {
            var contract = await _messagesRepository.GetByIdAsync(partnerMessageId);

            if (contract == null)
            {
                _log.Info($"Partner message with id {partnerMessageId} not found");
                return null;
            }

            var content = await _messageContentRepository.GetContentAsync(partnerMessageId);

            contract.Message = content;

            return contract;
        }

        public async Task DeleteMessageAsync(string partnerMessageId)
        {
            await _messagesRepository.DeleteMessageAsync(partnerMessageId);

            _log.Info("Message deleted from database", partnerMessageId);

            await _messageContentRepository.DeleteContentAsync(partnerMessageId);

            _log.Info("Message content deleted from database", partnerMessageId);
        }
    }
}

using System;
using System.Threading.Tasks;
using AutoFixture;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.CustomerManagement.Client;
using Lykke.Service.CustomerManagement.Client.Enums;
using Lykke.Service.NotificationSystem.SubscriberContract;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using Lykke.Service.PartnersIntegration.Domain.Enums;
using Lykke.Service.PartnersIntegration.Domain.Helpers;
using Lykke.Service.PartnersIntegration.Domain.Models;
using Lykke.Service.PartnersIntegration.Domain.Repositories;
using Lykke.Service.PartnersIntegration.Domain.Services;
using Lykke.Service.PartnersIntegration.DomainServices.Services;
using Moq;
using Xunit;

namespace Lykke.Service.PartnersIntegration.Tests
{
    public class MessagesServiceTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IMessagesRepository> _messagesRepositoryMock = new Mock<IMessagesRepository>();

        private readonly Mock<IMessageContentRepository> _messageContentRepositoryMock =
            new Mock<IMessageContentRepository>();

        private readonly Mock<IPartnerAndLocationHelper> _partnerAndLocationHelperMock =
            new Mock<IPartnerAndLocationHelper>();

        private readonly Mock<ICustomerManagementServiceClient> _customerManagementServiceClientMock =
            new Mock<ICustomerManagementServiceClient>();

        private readonly Mock<IRabbitPublisher<PushNotificationEvent>> _partnerMessagesPublisherMock =
            new Mock<IRabbitPublisher<PushNotificationEvent>>();

        private readonly Mock<IPartnerManagementClient> _partnerManagementClientMock =
            new Mock<IPartnerManagementClient>();

        private readonly IMessagesService _service;
        private const string PartnerMessageId = "dummy partner message id";

        public MessagesServiceTests()
        {
            _service = new MessagesService(
                _messagesRepositoryMock.Object,
                _messageContentRepositoryMock.Object,
                _partnerAndLocationHelperMock.Object,
                _customerManagementServiceClientMock.Object,
                _partnerMessagesPublisherMock.Object,
                _partnerManagementClientMock.Object,
                "mock-template",
                EmptyLogFactory.Instance);
        }

        [Fact]
        public async Task When_Get_Message_Async_Is_Executed_For_Non_Existing_Partner_Message_Id_Then_Null_Is_Returned()
        {
            var result = await _service.GetMessageAsync(PartnerMessageId);

            Assert.Null(result);
        }

        [Fact]
        public async Task
            When_Get_Message_Async_Is_Executed_For_Existing_Partner_Message_Id_Then_Messages_And_Message_Content_Repositories_Are_Called()
        {
            _messagesRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Create<MessageGetResponse>()));

            _messageContentRepositoryMock.Setup(x => x.GetContentAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(It.IsAny<string>()));

            await _service.GetMessageAsync(PartnerMessageId);

            _messagesRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<string>()), Times.Once);
            _messageContentRepositoryMock.Verify(x => x.GetContentAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Delete_Message_Async_Is_Executed_For_Existing_Partner_Message_Id_Then_Messages_And_Message_Content_Repositories_Are_Called()
        {
            _messagesRepositoryMock.Setup(x => x.DeleteMessageAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _messageContentRepositoryMock.Setup(x => x.DeleteContentAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(It.IsAny<bool>()));

            await _service.DeleteMessageAsync(PartnerMessageId);

            _messagesRepositoryMock.Verify(x => x.DeleteMessageAsync(It.IsAny<string>()), Times.Once);
            _messageContentRepositoryMock.Verify(x => x.DeleteContentAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task When_Send_Message_Async_Is_Executed_For_Non_Existing_Customer_Then_Proper_Status_Is_Returned()
        {
            var contract = _fixture.Create<MessagesPostRequest>();

            _customerManagementServiceClientMock
                .Setup(x => x.CustomersApi.GetCustomerBlockStateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture
                    .Build<CustomerManagement.Client.Models.Responses.CustomerBlockStatusResponse>()
                    .With(x => x.Error, CustomerBlockStatusError.CustomerNotFound)
                    .Create()));

            var result = await _service.SendMessageAsync(contract);

            Assert.Null(result.PartnerMessageId);
            Assert.Equal(MessagesErrorCode.CustomerNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task When_Send_Message_Async_Is_Executed_For_Blocked_Customer_Then_Proper_Status_Is_Returned()
        {
            var contract = _fixture.Create<MessagesPostRequest>();

            _customerManagementServiceClientMock
                .Setup(x => x.CustomersApi.GetCustomerBlockStateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture
                    .Build<CustomerManagement.Client.Models.Responses.CustomerBlockStatusResponse>()
                    .With(x => x.Status, CustomerActivityStatus.Blocked)
                    .Create()));

            var result = await _service.SendMessageAsync(contract);

            Assert.Null(result.PartnerMessageId);
            Assert.Equal(MessagesErrorCode.CustomerIsBlocked, result.ErrorCode);
        }

        [Fact]
        public async Task When_Send_Message_Async_Is_Executed_For_Non_Existing_Partner_Then_Proper_Status_Is_Returned()
        {
            var contract = _fixture.Create<MessagesPostRequest>();

            _customerManagementServiceClientMock
                .Setup(x => x.CustomersApi.GetCustomerBlockStateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture
                    .Build<CustomerManagement.Client.Models.Responses.CustomerBlockStatusResponse>()
                    .Create()));

            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.PartnerNotFound));

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Create<LocationInfoResponse>()));

            var result = await _service.SendMessageAsync(contract);

            Assert.Null(result.PartnerMessageId);
            Assert.Equal(MessagesErrorCode.PartnerNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task When_Send_Message_Async_Is_Executed_For_Non_Existing_Location_Then_Proper_Status_Is_Returned()
        {
            var contract = _fixture.Create<MessagesPostRequest>();

            _customerManagementServiceClientMock
                .Setup(x => x.CustomersApi.GetCustomerBlockStateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture
                    .Create<CustomerManagement.Client.Models.Responses.CustomerBlockStatusResponse>()));

            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.LocationNotFound));

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Create<LocationInfoResponse>()));

            var result = await _service.SendMessageAsync(contract);

            Assert.Null(result.PartnerMessageId);
            Assert.Equal(MessagesErrorCode.LocationNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task When_Send_Message_Async_Is_Executed_Then_Messages_And_Message_Content_Repositories_Are_Called()
        {
            var contract = _fixture.Create<MessagesPostRequest>();

            _customerManagementServiceClientMock
                .Setup(x => x.CustomersApi.GetCustomerBlockStateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture
                    .Build<CustomerManagement.Client.Models.Responses.CustomerBlockStatusResponse>()
                    .With(x => x.Error, CustomerBlockStatusError.None)
                    .With(x => x.Status, CustomerActivityStatus.Active)
                    .Create()));

            _partnerAndLocationHelperMock
                .Setup(x => x.GetPartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(_fixture
                    .Build<PartnerInfo>()
                    .With(x => x.PartnerAndLocationStatus, PartnerAndLocationStatus.OK)
                    .Create()
                ));

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Create<LocationInfoResponse>()));

            _messagesRepositoryMock.Setup(x => x.InsertAsync(contract))
                .Returns(Task.FromResult(Guid.NewGuid()));

            _messageContentRepositoryMock.Setup(x => x.SaveContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _service.SendMessageAsync(contract);

            _messagesRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<MessagesPostRequest>()), Times.Once);
            _messageContentRepositoryMock.Verify(x => x.SaveContentAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task When_Send_Message_Async_Is_Executed_With_Send_Push_Notification_Set_To_False_Then_Partner_Message_Publisher_Is_Not_Called()
        {
            var contract = _fixture
                .Build<MessagesPostRequest>()
                .With(x => x.SendPushNotification, false)
                .Create();

            _customerManagementServiceClientMock
                .Setup(x => x.CustomersApi.GetCustomerBlockStateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture
                    .Build<CustomerManagement.Client.Models.Responses.CustomerBlockStatusResponse>()
                    .With(x => x.Error, CustomerBlockStatusError.None)
                    .With(x => x.Status, CustomerActivityStatus.Active)
                    .Create()));

            _partnerAndLocationHelperMock
                .Setup(x => x.GetPartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(_fixture
                    .Build<PartnerInfo>()
                    .With(x => x.PartnerAndLocationStatus, PartnerAndLocationStatus.OK)
                    .Create()
                ));

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Create<LocationInfoResponse>()));

            _messagesRepositoryMock.Setup(x => x.InsertAsync(contract))
                .Returns(Task.FromResult(Guid.NewGuid()));

            _messageContentRepositoryMock.Setup(x => x.SaveContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _service.SendMessageAsync(contract);

            _partnerMessagesPublisherMock.Verify(
                x => x.PublishAsync(It.IsAny<PushNotificationEvent>()), Times.Never);
        }

        [Fact]
        public async Task When_Send_Message_Async_Is_Executed_With_Send_Push_Notification_Set_To_True_Then_Partner_Message_Publisher_Is_Not_Called()
        {
            var contract = _fixture
                .Build<MessagesPostRequest>()
                .With(x => x.SendPushNotification, true)
                .Create();

            _customerManagementServiceClientMock
                .Setup(x => x.CustomersApi.GetCustomerBlockStateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture
                    .Build<CustomerManagement.Client.Models.Responses.CustomerBlockStatusResponse>()
                    .With(x => x.Error, CustomerBlockStatusError.None)
                    .With(x => x.Status, CustomerActivityStatus.Active)
                    .Create()));

            _partnerAndLocationHelperMock
                .Setup(x => x.GetPartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(_fixture
                    .Build<PartnerInfo>()
                    .With(x => x.PartnerAndLocationStatus, PartnerAndLocationStatus.OK)
                    .Create()
                ));

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Create<LocationInfoResponse>()));

            _messagesRepositoryMock.Setup(x => x.InsertAsync(contract))
                .Returns(Task.FromResult(Guid.NewGuid()));

            _messageContentRepositoryMock.Setup(x => x.SaveContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _service.SendMessageAsync(contract);

            _partnerMessagesPublisherMock.Verify(
                x => x.PublishAsync(It.IsAny<PushNotificationEvent>()), Times.Once);
        }

        [Fact]
        public async Task When_Send_Message_Async_Is_Executed_And_Content_Is_Not_Saved_Due_To_Exception_Then_Exception_Is_Thrown()
        {
            var contract = _fixture.Create<MessagesPostRequest>();

            _customerManagementServiceClientMock
                .Setup(x => x.CustomersApi.GetCustomerBlockStateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture
                    .Build<CustomerManagement.Client.Models.Responses.CustomerBlockStatusResponse>()
                    .With(x => x.Error, CustomerBlockStatusError.None)
                    .With(x => x.Status, CustomerActivityStatus.Active)
                    .Create()));

            _partnerAndLocationHelperMock
                .Setup(x => x.GetPartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(_fixture
                    .Build<PartnerInfo>()
                    .With(x => x.PartnerAndLocationStatus, PartnerAndLocationStatus.OK)
                    .Create()
                ));

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Create<LocationInfoResponse>()));

            _messagesRepositoryMock.Setup(x => x.InsertAsync(contract))
                .Returns(Task.FromResult(Guid.NewGuid()));

            _messagesRepositoryMock.Setup(x => x.DeleteMessageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            _messageContentRepositoryMock.Setup(x => x.SaveContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("dummy exception"));

            await Assert.ThrowsAsync<Exception>(() => _service.SendMessageAsync(contract));
        }
    }
}

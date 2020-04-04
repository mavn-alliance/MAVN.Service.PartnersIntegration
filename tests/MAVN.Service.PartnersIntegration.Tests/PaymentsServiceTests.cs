using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Lykke.Logs;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using MAVN.Service.PartnersIntegration.Domain.Enums;
using MAVN.Service.PartnersIntegration.Domain.Helpers;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.Domain.Repositories;
using MAVN.Service.PartnersIntegration.Domain.Services;
using MAVN.Service.PartnersIntegration.DomainServices.Services;
using Lykke.Service.PartnersPayments.Client;
using Lykke.Service.PartnersPayments.Client.Enums;
using Lykke.Service.PartnersPayments.Client.Models;
using Moq;
using Xunit;
using PaymentRequestStatus = Lykke.Service.PartnersIntegration.Domain.Enums.PaymentRequestStatus;

namespace MAVN.Service.PartnersIntegration.Tests
{
    public class PaymentsServiceTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IMapper _mapper;

        private readonly Mock<IPartnerAndLocationHelper> _partnerAndLocationHelperMock =
            new Mock<IPartnerAndLocationHelper>();

        private readonly Mock<IPartnersPaymentsClient>
            _partnersPaymentsClientMock = new Mock<IPartnersPaymentsClient>();

        private readonly Mock<IPartnerManagementClient> _partnerManagementClientMock =
            new Mock<IPartnerManagementClient>();

        private readonly Mock<IMessagesService> _messagesServiceMock = new Mock<IMessagesService>();

        private readonly Mock<IPaymentCallbackRepository> _paymentCallbackRepositoryMock =
            new Mock<IPaymentCallbackRepository>();

        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        private readonly PaymentsService _service;

        public PaymentsServiceTests()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DomainServices.AutoMapperProfile());
            });
            _mapper = mockMapper.CreateMapper();

            _service = new PaymentsService(_mapper, EmptyLogFactory.Instance, _partnerAndLocationHelperMock.Object,
                _partnersPaymentsClientMock.Object, _partnerManagementClientMock.Object, _messagesServiceMock.Object,
                _paymentCallbackRepositoryMock.Object, _httpClientFactoryMock.Object, 1);
        }

        [Fact]
        public void
            When_Validate_Create_Payment_Request_Is_Executed_And_No_Fiat_And_Tokens_Amount_Is_Provided_Then_Exception_Is_Thrown()
        {
            var request = _fixture.Build<PaymentsCreateRequest>().With(x => x.FiatAmount, (decimal?)null)
                .With(x => x.TokensAmount, (decimal?)null).Create();

            var ex = Assert.Throws<ArgumentNullException>(() => _service.ValidateCreatePaymentRequest(request));

            Assert.Contains("Fiat or Token amount required", ex.Message);
            Assert.Equal(nameof(request.FiatAmount), ex.ParamName);
        }

        [Fact]
        public void
            When_Validate_Create_Payment_Request_Is_Executed_And_No_Customer_Id_Is_Provided_Then_Exception_Is_Thrown()
        {
            var request = _fixture.Build<PaymentsCreateRequest>().With(x => x.CustomerId, string.Empty)
                .Create();

            var ex = Assert.Throws<ArgumentNullException>(() => _service.ValidateCreatePaymentRequest(request));

            Assert.Equal(nameof(request.CustomerId), ex.ParamName);
        }

        [Fact]
        public void
            When_Validate_Create_Payment_Request_Is_Executed_And_No_Partner_Id_Is_Provided_Then_Exception_Is_Thrown()
        {
            var request = _fixture.Build<PaymentsCreateRequest>().With(x => x.PartnerId, string.Empty)
                .Create();

            var ex = Assert.Throws<ArgumentNullException>(() => _service.ValidateCreatePaymentRequest(request));

            Assert.Equal(nameof(request.PartnerId), ex.ParamName);
        }

        [Fact]
        public void
            When_Validate_Create_Payment_Request_Is_Executed_And_No_Tokens_Amount_Is_Provided_And_Provided_Fiat_Amount_IsNegative_Then_Exception_Is_Thrown()
        {
            var request = _fixture.Build<PaymentsCreateRequest>().With(x => x.TokensAmount, (decimal?)null)
                .With(x => x.FiatAmount, -1).Create();

            var ex = Assert.Throws<ArgumentException>(() => _service.ValidateCreatePaymentRequest(request));

            Assert.Equal("FiatAmount must be a positive number", ex.Message);
        }

        [Fact]
        public void
            When_Validate_Create_Payment_Request_Is_Executed_And_No_Tokens_Amount_Is_Provided_And_No_Currency_Is_Provided_Then_Exception_Is_Thrown()
        {
            var request = _fixture.Build<PaymentsCreateRequest>().With(x => x.TokensAmount, (decimal?)null)
                .With(x => x.Currency, string.Empty).Create();

            var ex = Assert.Throws<ArgumentNullException>(() => _service.ValidateCreatePaymentRequest(request));

            Assert.Equal(nameof(request.Currency), ex.ParamName);
        }

        [Fact]
        public void
            When_Validate_Create_Payment_Request_Is_Executed_And_No_Tokens_Amount_Is_Provided_And_Currency_Length_Is_Too_Long_Then_Exception_Is_Thrown()
        {
            var request = _fixture.Build<PaymentsCreateRequest>().With(x => x.TokensAmount, (decimal?)null)
                .With(x => x.Currency, "a".PadLeft(21, 'a')).Create();

            var ex = Assert.Throws<ArgumentException>(() => _service.ValidateCreatePaymentRequest(request));

            Assert.Equal($"{nameof(request.Currency)} must not be bigger than 20 characters", ex.Message);
        }

        [Fact]
        public void
            When_Validate_Create_Payment_Request_Is_Executed_And_No_Fiat_Amount_Is_Provided_And_Tokens_Amount_Is_Negative_Then_Exception_Is_Thrown()
        {
            var request = _fixture.Build<PaymentsCreateRequest>().With(x => x.FiatAmount, (decimal?)null)
                .With(x => x.TokensAmount, -1).Create();

            var ex = Assert.Throws<ArgumentException>(() => _service.ValidateCreatePaymentRequest(request));

            Assert.Equal("TokensAmount must be a positive number", ex.Message);
        }

        [Fact]
        public async Task
            When_Get_Payment_Request_Status_Async_Is_Executed_And_No_Payment_Request_Id_Is_Provided_Then_Exception_Is_Thrown()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.GetPaymentRequestStatusAsync(null, "a"));

            Assert.Equal("paymentRequestId", ex.ParamName);
        }

        [Fact]
        public async Task
            When_Get_Payment_Request_Status_Async_Is_Executed_And_Payment_Request_Id_Is_Provided_Then_Partners_Payments_Client_Is_Called()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture
                    .Create<PaymentDetailsResponseModel>()));

            await _service.GetPaymentRequestStatusAsync("1", "2");

            _partnersPaymentsClientMock.Verify(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Get_Payment_Request_Status_Async_Is_Executed_And_Non_Existing_Payment_Request_Id_Is_Provided_Then_Proper_Response_Is_Returned()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<PaymentDetailsResponseModel>(null));

            var result = await _service.GetPaymentRequestStatusAsync("1", "2");

            Assert.Equal(PaymentRequestStatus.PaymentRequestNotFound, result.Status);
        }

        [Fact]
        public async Task
            When_Get_Payment_Request_Status_Async_Is_Executed_And_Payment_Request_Id_Is_Provided_But_Does_Not_Correspond_To_Provided_Partner_Id_Then_Proper_Response_Is_Returned()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentDetailsResponseModel>()
                    .With(x => x.PartnerId, "3").Create()));

            var result = await _service.GetPaymentRequestStatusAsync("1", "2");

            Assert.Equal(PaymentRequestStatus.PaymentRequestNotFound, result.Status);
        }

        [Fact]
        public async Task
            When_Cancel_Payment_Request_Async_Is_Executed_And_No_Payment_Request_Id_Is_Provided_Then_Exception_Is_Thrown()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.CancelPaymentRequestAsync(null, "a"));

            Assert.Equal("paymentRequestId", ex.ParamName);
        }

        [Fact]
        public async Task
            When_Cancel_Payment_Request_Async_Is_Executed_And_Not_Existing_Payment_Request_Id_Is_Provided_Then_Partners_Payments_Client_Is_Called()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((PaymentDetailsResponseModel)null));

            await _service.CancelPaymentRequestAsync("1", "2");

            _partnersPaymentsClientMock.Verify(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()), Times.Once);
            _partnersPaymentsClientMock.Verify(
                x => x.Api.PartnerCancelPaymentAsync(It.IsAny<ReceptionistProcessPaymentRequest>()), Times.Never);
        }

        [Fact]
        public async Task
            When_Cancel_Payment_Request_Async_Is_Executed_And_Valid_Payment_Request_Id_And_Non_Corresponding_Payment_Id_Are_Provided_Then_Partners_Payments_Client_Is_Called()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Create<PaymentDetailsResponseModel>()));

            await _service.CancelPaymentRequestAsync("1", "2");

            _partnersPaymentsClientMock.Verify(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()), Times.Once);
            _partnersPaymentsClientMock.Verify(
                x => x.Api.PartnerCancelPaymentAsync(It.IsAny<ReceptionistProcessPaymentRequest>()), Times.Never);
        }

        [Fact]
        public async Task
            When_Cancel_Payment_Request_Async_Is_Executed_And_Valid_Payment_Request_Id_And_Corresponding_Payment_Id_Are_Provided_Then_Partners_Payments_Client_Is_Called()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentDetailsResponseModel>().With(x => x.PartnerId, "2")
                    .Create()));

            await _service.CancelPaymentRequestAsync("1", "2");

            _partnersPaymentsClientMock.Verify(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()), Times.Once);
            _partnersPaymentsClientMock.Verify(
                x => x.Api.PartnerCancelPaymentAsync(It.IsAny<ReceptionistProcessPaymentRequest>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Execute_Payment_Request_Async_Is_Executed_And_No_Payment_Request_Id_Is_Provided_Then_Exception_Is_Thrown()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.ExecutePaymentRequestAsync(_fixture.Build<PaymentsExecuteRequest>()
                    .With(x => x.PaymentRequestId, string.Empty).Create()));

            Assert.Equal("PaymentRequestId", ex.ParamName);
        }

        [Fact]
        public async Task
            When_Execute_Payment_Request_Async_Is_Executed_And_Non_Existing_Payment_Request_Id_Is_Provided_Then_Proper_Response_Is_Returned()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((PaymentDetailsResponseModel)null));

            var result = await _service.ExecutePaymentRequestAsync(_fixture.Create<PaymentsExecuteRequest>());

            Assert.Equal(PaymentExecuteStatus.PaymentRequestNotFound, result.Status);
        }

        [Fact]
        public async Task
            When_Execute_Payment_Request_Async_Is_Executed_And_Existing_Payment_Request_Id_With_Non_Corresponding_Partner_Id_Are_Provided_Then_Proper_Response_Is_Returned()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentDetailsResponseModel>().With(x => x.PartnerId, "1")
                    .Create()));

            var result = await _service.ExecutePaymentRequestAsync(_fixture.Build<PaymentsExecuteRequest>()
                .With(x => x.PartnerId, "2").Create());

            Assert.Equal(PaymentExecuteStatus.PaymentRequestNotFound, result.Status);
        }

        [Fact]
        public async Task
            When_Execute_Payment_Request_Async_Is_Executed_And_Existing_Payment_Request_Id_With_Corresponding_Partner_Id_Are_Provided_Then_Partners_Payments_Client_Is_Called()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentDetailsResponseModel>().With(x => x.PartnerId, "1")
                    .Create()));

            _partnersPaymentsClientMock.Setup(x =>
                    x.Api.ReceptionistApprovePaymentAsync(It.IsAny<ReceptionistProcessPaymentRequest>()))
                .Returns(Task.FromResult(_fixture.Create<PaymentStatusUpdateResponse>()));

            await _service.ExecutePaymentRequestAsync(_fixture.Build<PaymentsExecuteRequest>()
                .With(x => x.PartnerId, "1").Create());

            _partnersPaymentsClientMock.Verify(
                x => x.Api.ReceptionistApprovePaymentAsync(It.IsAny<ReceptionistProcessPaymentRequest>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Execute_Payment_Request_Async_Is_Executed_And_Existing_Payment_Request_Id_With_Corresponding_Partner_Id_Are_Provided_And_Receptions_Payment_Approval_Failed_Then_Proper_Response_Is_Returned()
        {
            _partnersPaymentsClientMock.Setup(x => x.Api.GetPaymentDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentDetailsResponseModel>().With(x => x.PartnerId, "1")
                    .Create()));

            _partnersPaymentsClientMock.Setup(x =>
                    x.Api.ReceptionistApprovePaymentAsync(It.IsAny<ReceptionistProcessPaymentRequest>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentStatusUpdateResponse>()
                    .With(x => x.Error, PaymentStatusUpdateErrorCodes.PaymentDoesNotExist).Create()));

            var result = await _service.ExecutePaymentRequestAsync(_fixture.Build<PaymentsExecuteRequest>()
                .With(x => x.PartnerId, "1").Create());

            _partnersPaymentsClientMock.Verify(
                x => x.Api.ReceptionistApprovePaymentAsync(It.IsAny<ReceptionistProcessPaymentRequest>()), Times.Once);

            Assert.Equal(PaymentExecuteStatus.PaymentRequestNotFound, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_External_Location_Id_Is_Provided_Then_Partner_Management_Client_Is_Called()
        {
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult((LocationInfoResponse)null));

            await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, "test").Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_External_Location_Id_Is_Provided_Then_Partner_And_Location_Helper_Is_Called()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.LocationNotFound));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);
            _partnerAndLocationHelperMock.Verify(
                x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_Then_Partner_And_Location_Helper_Is_Called()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.LocationNotFound));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            _partnerAndLocationHelperMock.Verify(
                x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_For_Non_Existing_Partner_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.PartnerNotFound));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.PartnerNotFound, result.Status);
            Assert.Null(result.PaymentRequestId);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_For_Non_Existing_Location_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.LocationNotFound));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.LocationNotFound, result.Status);
            Assert.Null(result.PaymentRequestId);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_With_Additional_Payment_Info_Then_Message_Service_Is_Called()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.CustomerIsBlocked).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            _messagesServiceMock.Verify(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_For_Blocked_Customer_Then_Proper_Message_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.CustomerIsBlocked).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.CustomerIsBlocked, result.Status);
            Assert.Null(result.PaymentRequestId);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_For_Non_Existing_Customer_Then_Proper_Message_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.CustomerNotFound).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.CustomerNotFound, result.Status);
            Assert.Null(result.PaymentRequestId);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_For_Non_Existing_Location_Then_Proper_Message_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.LocationNotFound).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.LocationNotFound, result.Status);
            Assert.Null(result.PaymentRequestId);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_For_Non_Existing_Partner_Then_Proper_Message_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.PartnerNotFound).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.PartnerNotFound, result.Status);
            Assert.Null(result.PaymentRequestId);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_Then_Partner_Payments_Client_Is_Called()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.None).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            _partnersPaymentsClientMock.Verify(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()),
                Times.Once);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Customer_Is_Not_Found_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.CannotPassBothFiatAndTokensAmount).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.CannotPassBothFiatAndTokensAmount, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Customer_Wallet_Is_Blocked_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.CustomerWalletBlocked).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.CustomerIsBlocked, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Both_Fiat_And_Tokens_Amount_Are_Passed_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.CannotPassBothFiatAndTokensAmount).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.CannotPassBothFiatAndTokensAmount, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Currency_Is_Invalid_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.InvalidCurrency).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.InvalidCurrency, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Neither_Fiat_Or_Tokens_Amount_Is_Provided_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.EitherFiatOrTokensAmountShouldBePassed).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.EitherFiatOrTokensAmountShouldBePassed, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Tokens_Amount_Is_Invalid_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.InvalidTokensAmount).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.InvalidTokensAmount, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Fiat_Amount_Is_Invalid_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.InvalidFiatAmount).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.InvalidFiatAmount, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Total_Bill_Amount_Is_Invalid_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.InvalidTotalBillAmount).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.InvalidTotalBillAmount, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Partner_Id_Is_Invalid_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.PartnerIdIsNotAValidGuid).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.PartnerNotFound, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Partner_Does_Not_Exist_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.PartnerDoesNotExist).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.PartnerNotFound, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_There_Is_No_Such_Location_For_Partner_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.NoSuchLocationForThisPartner).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.LocationNotFound, result.Status);
        }

        [Fact]
        public async Task
            When_Create_Payment_Request_Async_Is_Executed_And_Partner_Payments_Client_Response_Is_That_Tokens_Or_Currency_Rate_Is_Invalid_Then_Proper_Response_Is_Returned()
        {
            _partnerAndLocationHelperMock
                .Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _messagesServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<MessagesPostRequest>()))
                .Returns(Task.FromResult(_fixture.Build<MessagesPostResponse>()
                    .With(x => x.ErrorCode, MessagesErrorCode.OK).Create()));

            _partnersPaymentsClientMock.Setup(x => x.Api.PartnerPaymentAsync(It.IsAny<PaymentRequestModel>()))
                .Returns(Task.FromResult(_fixture.Build<PaymentRequestResponseModel>()
                    .With(x => x.Error, PaymentRequestErrorCodes.InvalidTokensOrCurrencyRateInPartner).Create()));

            var externalLocationId = "extLoc";
            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(new LocationInfoResponse
                {
                    Id = Guid.NewGuid()
                }));

            var result = await _service.CreatePaymentRequestAsync(_fixture.Build<PaymentsCreateRequest>()
                .With(x => x.ExternalLocationId, externalLocationId).Create());

            _partnerManagementClientMock.Verify(x => x.Locations.GetByExternalId2Async(externalLocationId), Times.Once);

            Assert.Equal(PaymentCreateStatus.InternalTechnicalError, result.Status);
        }
    }
}

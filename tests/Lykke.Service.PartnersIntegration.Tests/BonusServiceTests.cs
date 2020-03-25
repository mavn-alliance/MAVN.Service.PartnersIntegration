using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using Lykke.Service.PartnersIntegration.Contract;
using Lykke.Service.PartnersIntegration.Domain.Enums;
using Lykke.Service.PartnersIntegration.Domain.Helpers;
using Lykke.Service.PartnersIntegration.Domain.Models;
using Lykke.Service.PartnersIntegration.DomainServices.Services;
using Lykke.Service.Referral.Client;
using Lykke.Service.Referral.Client.Enums;
using Lykke.Service.Referral.Client.Models.Requests;
using Lykke.Service.Referral.Client.Models.Responses;
using Moq;
using Xunit;

namespace Lykke.Service.PartnersIntegration.Tests
{
    public class BonusServiceTests
    {
        private readonly Mock<ICustomerProfileClient> _customerProfileClientMock
            = new Mock<ICustomerProfileClient>();

        private readonly Mock<IRabbitPublisher<BonusCustomerTriggerEvent>> _bonusCustomerEventPublisherMock
            = new Mock<IRabbitPublisher<BonusCustomerTriggerEvent>>();

        private readonly Mock<IReferralClient> _referralClientMock
            = new Mock<IReferralClient>();

        private readonly Mock<IPartnerAndLocationHelper> _partnerAndLocationHelperMock
            = new Mock<IPartnerAndLocationHelper>();

        private readonly Mock<IPartnerManagementClient> _partnerManagementClientMock
            = new Mock<IPartnerManagementClient>();

        private readonly BonusService _service;

        public BonusServiceTests()
        {
            _service = new BonusService(
                EmptyLogFactory.Instance,
                _customerProfileClientMock.Object,
                _bonusCustomerEventPublisherMock.Object,
                _partnerAndLocationHelperMock.Object,
                _referralClientMock.Object,
                _partnerManagementClientMock.Object);
        }
        
        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_CalledWithValidData_ExpectGetByCustomerIdAsyncCalled()
        {
            //Arrange
            var customerId = "customerId";
            var email = "email";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = customerId,
                    Email = email,
                    PartnerId = "partnerId",
                    Currency = "AED",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse {Id = Guid.NewGuid()}));
            
            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(customerId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile { Email = email }
                }));

            _referralClientMock.Setup(x => x.ReferralHotelsApi.UseAsync(It.IsAny<ReferralHotelUseRequest>()))
                .Returns(Task.FromResult(new ReferralHotelUseResponse {ErrorCode = ReferralHotelUseErrorCode.None}));

            _bonusCustomerEventPublisherMock.Setup(x => x.PublishAsync(It.IsAny<BonusCustomerTriggerEvent>()));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert
            _customerProfileClientMock.Verify(x => x.CustomerProfiles.GetByCustomerIdAsync(customerId, false, false), Times.Once);

            _referralClientMock.Verify(x => x.ReferralHotelsApi.UseAsync(It.IsAny<ReferralHotelUseRequest>()), Times.Once);

            _bonusCustomerEventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<BonusCustomerTriggerEvent>()), Times.Once);

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.OK, response[0].Status);
            Assert.Equal(customerId, response[0].CustomerId);
            Assert.Equal(1, response[0].BonusCustomerSeqNumber);
            Assert.Equal(email, response[0].CustomerEmail);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_CalledWithMissingCustomerId_ExpectCustomerNotFoundStatus()
        {
            //Arrange
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = null,
                    Email = null,
                    PartnerId = "partnerId",
                    Currency = "currency",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.CustomerNotFound, response[0].Status);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_CalledWithMissingPartnerId_ExpectPartnerNotFoundStatus()
        {
            //Arrange
            var customerId = "customerId";
            var email = "email";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = customerId,
                    Email = email,
                    PartnerId = null,
                    Currency = "currency",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.PartnerNotFound, response[0].Status);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_CalledWithMissingFiatAmount_ExpectInvalidFiatAmountStatus()
        {
            //Arrange
            var customerId = "customerId";
            var email = "email";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = customerId,
                    Email = email,
                    PartnerId = "partnerId",
                    Currency = "currency",
                    ExternalLocationId = "locationId",
                    FiatAmount = null,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.InvalidFiatAmount, response[0].Status);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_CalledWithMissingCurrency_ExpectInvalidCurrencyStatus()
        {
            //Arrange
            var customerId = "customerId";
            var email = "email";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = customerId,
                    Email = email,
                    PartnerId = "partnerId",
                    Currency = null,
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.InvalidCurrency, response[0].Status);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_CalledWithMissingPaymentTimestamp_ExpectInvalidPaymentTimestampStatus()
        {
            //Arrange
            var customerId = "customerId";
            var email = "email";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = customerId,
                    Email = email,
                    PartnerId = "partnerId",
                    Currency = "AED",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = null,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.InvalidPaymentTimestamp, response[0].Status);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_MissingCustomerProfile_ExpectCustomerNotFoundStatus()
        {
            //Arrange
            var customerId = "customerId";
            var email = "email";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = customerId,
                    Email = email,
                    PartnerId = "partnerId",
                    Currency = "AED",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(customerId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = null
                }));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.CustomerNotFound, response[0].Status);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_AndCustomerProfileEmailDoesNotMatchEmail_ExpectCustomerIdDoesNotMatchEmailStatus()
        {
            //Arrange
            var customerId = "customerId";
            var email = "email";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = customerId,
                    Email = email,
                    PartnerId = "partnerId",
                    Currency = "AED",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(customerId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile { Email = email + "Wrong"}
                }));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.CustomerIdDoesNotMatchEmail, response[0].Status);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_AndEmailIsMissing_ExpectEmailSetFromProfile()
        {
            //Arrange
            var customerId = "customerId";
            var email = "email";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = customerId,
                    Email = null,
                    PartnerId = "partnerId",
                    Currency = "AED",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(customerId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile { Email =  email}
                }));

            _referralClientMock.Setup(x => x.ReferralHotelsApi.UseAsync(It.IsAny<ReferralHotelUseRequest>()))
                .Returns(Task.FromResult(new ReferralHotelUseResponse { ErrorCode = ReferralHotelUseErrorCode.None }));

            _bonusCustomerEventPublisherMock.Setup(x => x.PublishAsync(It.IsAny<BonusCustomerTriggerEvent>()));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.OK, response[0].Status);
            Assert.Equal(email, response[0].CustomerEmail);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_AndEmailIsMissingAndProfileNotFound_ExpectCustomerNotFoundStatus()
        {
            //Arrange
            var customerId = "customerId";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = customerId,
                    Email = null,
                    PartnerId = "partnerId",
                    Currency = "AED",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(customerId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = null
                }));
            
            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.CustomerNotFound, response[0].Status);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_AndCustomerIdIsMissingAndProfileNotFound_ExpectCustomerNotFoundStatus()
        {
            //Arrange
            var email = "mail";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = null,
                    Email = email,
                    PartnerId = "partnerId",
                    Currency = "AED",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = null
                }));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert
            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.CustomerNotFound, response[0].Status);
        }

        [Fact]
        public async Task When_TriggerBonusToCustomersAsync_AndCustomerIdIsMissing_ExpectCustomerIdSetFromProfile()
        {
            //Arrange
            var customerId = "customerId";
            var email = "email";
            var contracts = new List<BonusCustomerRequest>
            {
                new BonusCustomerRequest
                {
                    CustomerId = null,
                    Email = email,
                    PartnerId = "partnerId",
                    Currency = "AED",
                    ExternalLocationId = "locationId",
                    FiatAmount = 1,
                    PaymentTimestamp = DateTime.UtcNow,
                    PosId = "PosId"
                }
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.Is<GetByEmailRequestModel>(i => i.Email == email)))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile { CustomerId = customerId }
                }));

            _referralClientMock.Setup(x => x.ReferralHotelsApi.UseAsync(It.IsAny<ReferralHotelUseRequest>()))
                .Returns(Task.FromResult(new ReferralHotelUseResponse { ErrorCode = ReferralHotelUseErrorCode.None }));

            _bonusCustomerEventPublisherMock.Setup(x => x.PublishAsync(It.IsAny<BonusCustomerTriggerEvent>()));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            //Act
            var response = await _service.TriggerBonusToCustomersAsync(contracts);

            //Assert

            Assert.Single(response);
            Assert.Equal(BonusCustomerStatus.OK, response[0].Status);
            Assert.Equal(customerId, response[0].CustomerId);
        }
    }
}

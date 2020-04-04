using System;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Logs;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.EligibilityEngine.Client.Enums;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Responses;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using MAVN.Service.PartnersIntegration.Domain.Enums;
using MAVN.Service.PartnersIntegration.Domain.Helpers;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.DomainServices.Services;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.Tiers.Client;
using Lykke.Service.Tiers.Client.Models.Tiers;
using Moq;
using Xunit;

namespace MAVN.Service.PartnersIntegration.Tests
{
    public class CustomersServiceTests
    {
        private readonly Mock<ICustomerProfileClient> _customerProfileClientMock
            = new Mock<ICustomerProfileClient>();

        private readonly Mock<IPrivateBlockchainFacadeClient> _privateBlockchainFacadeClientMock
            = new Mock<IPrivateBlockchainFacadeClient>();

        private readonly Mock<IPartnerAndLocationHelper> _partnerAndLocationHelperMock
            = new Mock<IPartnerAndLocationHelper>();

        private readonly Mock<IMapper> _mapperMock
            = new Mock<IMapper>();

        private readonly Mock<ITiersClient> _tiersMock
            = new Mock<ITiersClient>();

        private readonly Mock<IPartnerManagementClient> _partnerManagementClientMock
            = new Mock<IPartnerManagementClient>();

        private readonly Mock<IEligibilityEngineClient> _eligibilityEngineClientMock =
            new Mock<IEligibilityEngineClient>();

        private readonly CustomersService _service;

        public CustomersServiceTests()
        {
            _service = new CustomersService(_customerProfileClientMock.Object, _mapperMock.Object,
                _privateBlockchainFacadeClientMock.Object, _partnerAndLocationHelperMock.Object,
                _tiersMock.Object, EmptyLogFactory.Instance, _partnerManagementClientMock.Object,
                _eligibilityEngineClientMock.Object);
        }

        [Fact]
        public async Task When_GetCustomerInformationAsync_CalledWithId_ExpectGetByCustomerIdAsyncCalled()
        {
            //Arrange
            var customerId = Guid.NewGuid();
            var customerIdString = customerId.ToString();
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(customerIdString, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile {CustomerId = customerIdString }
                }));
            _tiersMock.Setup(x => x.Customers.GetTierAsync(customerId))
                .Returns(Task.FromResult(new TierModel()));

            //Act
            var response = await _service
                .GetCustomerInformationAsync(new CustomerInformationRequest {Id = customerIdString });

            //Assert
            Assert.Equal(CustomerInformationStatus.OK ,response.Status);
            Assert.Equal(customerIdString, response.CustomerId);
        }

        [Fact]
        public async Task When_GetCustomerInformationAsync_CalledWithUnknownId_ExpectCustomerNotFoundStatus()
        {
            //Arrange
            var customerId = "aidii";
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(customerId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = null
                }));

            //Act
            var response = await _service
                .GetCustomerInformationAsync(new CustomerInformationRequest {Id = customerId});

            //Assert
            Assert.Equal(CustomerInformationStatus.CustomerNotFound ,response.Status);
        }

        [Fact]
        public async Task When_GetCustomerInformationAsync_CalledWithEmail_ExpectGetByEmailAsyncCalled()
        {
            //Arrange
            var email = "emaiil";
            var customerId = Guid.NewGuid();
            var customerIdString = customerId.ToString();
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.Is<GetByEmailRequestModel>(i => i.Email == email)))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile {CustomerId = customerIdString }
                }));
            _tiersMock.Setup(x => x.Customers.GetTierAsync(customerId))
                .Returns(Task.FromResult(new TierModel()));
            
            //Act
            var response = await _service
                .GetCustomerInformationAsync(new CustomerInformationRequest {Email = email});

            //Assert
            Assert.Equal(CustomerInformationStatus.OK ,response.Status);
            Assert.Equal(customerIdString, response.CustomerId);
        }

        [Fact]
        public async Task When_GetCustomerInformationAsync_CalledWithUnknownEmail_ExpectCustomerNotFoundStatus()
        {
            //Arrange
            var email = "emaiil";
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.Is<GetByEmailRequestModel>(i => i.Email == email)))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = null
                }));

            //Act
            var response = await _service
                .GetCustomerInformationAsync(new CustomerInformationRequest {Email = email});

            //Assert
            Assert.Equal(CustomerInformationStatus.CustomerNotFound ,response.Status);
        }

        [Fact]
        public async Task When_GetCustomerInformationAsync_CalledWithPhone_ExpectGetByPhoneAsyncCalled()
        {
            //Arrange
            var phone = "phoone";
            var customerId = Guid.NewGuid();
            var customerIdString = customerId.ToString();
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByPhoneAsync(It.Is<GetByPhoneRequestModel>(i => i.Phone == phone)))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile {CustomerId = customerIdString }
                }));
            _tiersMock.Setup(x => x.Customers.GetTierAsync(customerId))
                .Returns(Task.FromResult(new TierModel()));

            //Act
            var response = await _service
                .GetCustomerInformationAsync(new CustomerInformationRequest {Phone = phone});

            //Assert
            Assert.Equal(CustomerInformationStatus.OK ,response.Status);
            Assert.Equal(customerIdString, response.CustomerId);
        }

        [Fact]
        public async Task When_GetCustomerInformationAsync_CalledWithUnknownPhone_ExpectCustomerNotFoundStatus()
        {
            //Arrange
            var phone = "phoone";
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByPhoneAsync(It.Is<GetByPhoneRequestModel>(i => i.Phone == phone)))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = null
                }));

            //Act
            var response = await _service
                .GetCustomerInformationAsync(new CustomerInformationRequest {Phone = phone});

            //Assert
            Assert.Equal(CustomerInformationStatus.CustomerNotFound ,response.Status);
        }

        [Fact]
        public async Task When_GetCustomerBalanceAsync_CalledWithValidData_ExpectCorrectResponse()
        {
            //Arrange
            var customerId = "f40f850a-fb88-4464-9672-da427d927c96";
            Guid.TryParse(customerId, out var customerIdGuid);
            var tokenCount = 12123;

            var partnerInfo = new PartnerInfo
            {
                PartnerAndLocationStatus = PartnerAndLocationStatus.OK,
                Id = Guid.NewGuid(),
                Name = "test partner"
            };

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(customerId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                }));

            _privateBlockchainFacadeClientMock.Setup(x => x.CustomersApi.GetBalanceAsync(customerIdGuid))
                .Returns(Task.FromResult(new CustomerBalanceResponseModel
                {
                    Error = CustomerBalanceError.None,
                    Total = tokenCount
                }));

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.GetPartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(partnerInfo));

            _eligibilityEngineClientMock.Setup(x =>
                    x.ConversionRate.ConvertOptimalByPartnerAsync(
                        It.IsAny<ConvertOptimalByPartnerRequest>()))
                .Returns(Task.FromResult(new ConvertOptimalByPartnerResponse
                {
                    ErrorCode = EligibilityEngineErrors.None,
                    ConversionSource = ConversionSource.Partner,
                    Amount = 123,
                    CurrencyCode = "test"
                }));

            //Act
            var response = await _service
                .GetCustomerBalanceAsync(customerId, new CustomerBalanceRequest
                {
                    PartnerId = Guid.NewGuid().ToString(),
                    ExternalLocationId = "locId",
                    Currency = "AED"
                });

            //Assert
            Assert.Equal(CustomerBalanceStatus.OK, response.Status);
            Assert.Equal(tokenCount, response.Tokens);
            Assert.Equal(123, response.FiatBalance);
        }

        [Fact]
        public async Task When_GetCustomerBalanceAsync_CalledWithUnknownCustomerId_ExpectCustomerNotFoundStatus()
        {
            //Arrange
            var customerId = "custId";

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(customerId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.CustomerProfileDoesNotExist
                }));

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(It.IsAny<string>()))
                .Returns(Task.FromResult(new LocationInfoResponse { Id = Guid.NewGuid() }));

            _partnerAndLocationHelperMock.Setup(x => x.GetPartnerInfo(It.IsAny<string>(), It.IsAny<LocationInfoResponse>()))
                .Returns(Task.FromResult(new PartnerInfo()));

            //Act
            var response = await _service
                .GetCustomerBalanceAsync(customerId, new CustomerBalanceRequest
                {
                    PartnerId = "partId",
                    ExternalLocationId = "locId",
                    Currency = "AED"
                });

            //Assert
            Assert.Equal(CustomerBalanceStatus.CustomerNotFound, response.Status);
        }

        [Fact]
        public async Task When_GetCustomerBalanceAsync_CalledWithMissingCustomerId_ExpectExceptionThrown()
        {
            //Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetCustomerBalanceAsync(null, new CustomerBalanceRequest()));
        }

        [Fact]
        public async Task When_GetCustomerBalanceAsync_CalledWithMissingPartnerId_ExpectExceptionThrown()
        {
            //Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetCustomerBalanceAsync("present", new CustomerBalanceRequest{PartnerId = null}));
        }

        [Fact]
        public async Task When_GetCustomerBalanceAsync_CalledWithMissingCurrency_ExpectExceptionThrown()
        {
            //Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetCustomerBalanceAsync("present", new CustomerBalanceRequest{PartnerId = "present", Currency = null}));
        }
    }
}

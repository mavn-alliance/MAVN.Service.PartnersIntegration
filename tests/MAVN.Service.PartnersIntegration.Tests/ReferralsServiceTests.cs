using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Logs;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using MAVN.Service.PartnersIntegration.Domain.Enums;
using MAVN.Service.PartnersIntegration.Domain.Helpers;
using MAVN.Service.PartnersIntegration.Domain.Models;
using MAVN.Service.PartnersIntegration.DomainServices.Services;
using Lykke.Service.Referral.Client;
using Lykke.Service.Referral.Client.Enums;
using Lykke.Service.Referral.Client.Models.Requests;
using Lykke.Service.Referral.Client.Models.Responses;
using Moq;
using Xunit;

namespace MAVN.Service.PartnersIntegration.Tests
{
    public class ReferralsServiceTests
    {
        private readonly Mock<ICustomerProfileClient> _customerProfileClientMock
            = new Mock<ICustomerProfileClient>();

        private readonly Mock<IReferralClient> _referralClientMock
            = new Mock<IReferralClient>();

        private readonly Mock<IPartnerAndLocationHelper> _partnerAndLocationHelperMock
            = new Mock<IPartnerAndLocationHelper>();

        private readonly Mock<IPartnerManagementClient> _partnerManagementClientMock
            = new Mock<IPartnerManagementClient>();

        private readonly Mock<IMapper> _mapperMock
            = new Mock<IMapper>();

        private readonly ReferralsService _service;

        public ReferralsServiceTests()
        {
            _service = new ReferralsService(EmptyLogFactory.Instance, _mapperMock.Object,
                _customerProfileClientMock.Object, _partnerAndLocationHelperMock.Object,
                _referralClientMock.Object, _partnerManagementClientMock.Object);
        }

        [Fact]
        public async Task When_GetReferralInformationAsync_CalledWithCorrectData_ExpectValidResult()
        {
            //Arrange
            var refereeId = Guid.NewGuid().ToString();
            var refereeEmail = "testmail@mail.com";

            var referrerId = Guid.NewGuid().ToString();
            var referrerEmail = "referrerEmail@email.com";
            var referrerFirstName = "FN";
            var referrerLastName = "LN";
            var referralHotelId = "aiDi";

            var partnerIdGuid = Guid.NewGuid();
            var externalLocationId = "ThisCanBeAnything-CoolRight?";

            var referralResponse = new ReferralHotelsListByEmailResponse
            {
                HotelReferrals = new List<ReferralHotelModel>
                {
                    new ReferralHotelModel
                    {
                        Email = refereeEmail,
                        ReferrerId = referrerId,
                        CreationDateTime = DateTime.UtcNow,
                        ExpirationDateTime = DateTime.UtcNow.AddDays(1),
                        State = ReferralHotelState.Pending,
                        Id = referralHotelId,
                        ConfirmationToken = "Pending"
                    },
                    new ReferralHotelModel
                    {
                        Email = refereeEmail,
                        ReferrerId = referrerId,
                        CreationDateTime = DateTime.UtcNow,
                        ExpirationDateTime = DateTime.UtcNow.AddDays(1),
                        State = ReferralHotelState.Confirmed,
                        Id = referralHotelId,
                        ConfirmationToken = "Confirmed"
                    },
                    new ReferralHotelModel
                    {
                        Email = refereeEmail,
                        ReferrerId = referrerId,
                        CreationDateTime = DateTime.UtcNow,
                        ExpirationDateTime = DateTime.UtcNow.AddDays(1),
                        State = ReferralHotelState.Used,
                        Id = referralHotelId,
                        ConfirmationToken = "Used"
                    },
                    new ReferralHotelModel
                    {
                        Email = refereeEmail,
                        ReferrerId = referrerId,
                        CreationDateTime = DateTime.UtcNow,
                        ExpirationDateTime = DateTime.UtcNow.AddDays(1),
                        State = ReferralHotelState.Expired,
                        Id = referralHotelId,
                        ConfirmationToken = "Expired"
                    }
                }
            };

            var locationInfoResponse = new LocationInfoResponse
            {
                Id = Guid.NewGuid(), PartnerId = partnerIdGuid, ExternalId = externalLocationId
            };

            _partnerManagementClientMock.Setup(x => x.Locations.GetByExternalId2Async(externalLocationId))
                .Returns(Task.FromResult(locationInfoResponse));

            _partnerAndLocationHelperMock.Setup(x => x.ValidatePartnerInfo(partnerIdGuid.ToString(), locationInfoResponse))
                .Returns(Task.FromResult(PartnerAndLocationStatus.OK));

            _referralClientMock.Setup(x => x.ReferralHotelsApi.GetByEmailAsync(It.Is<GetHotelReferralsByEmailRequestModel>(r =>
                    r.Email == refereeEmail
                    && r.PartnerId == partnerIdGuid.ToString()
                    && r.Location == locationInfoResponse.Id.ToString())))
                .Returns(Task.FromResult(referralResponse));

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(referrerId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.None,
                    Profile = new CustomerProfile
                    {
                        Email = referrerEmail,
                        FirstName = referrerFirstName,
                        LastName = referrerLastName
                    }
                }));

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(refereeId, false, false))
                .Returns(Task.FromResult(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.None,
                    Profile = new CustomerProfile
                    {
                        Email = refereeEmail
                    }
                }));

            //Act
            var response = await _service.GetReferralInformationAsync(new ReferralInformationRequest
            {
                CustomerId = refereeId, PartnerId = partnerIdGuid.ToString(), ExternalLocationId = externalLocationId
            });

            //Assert
            _referralClientMock.Verify(x => x.ReferralHotelsApi.GetByEmailAsync(It.Is<GetHotelReferralsByEmailRequestModel>(r =>
                r.Email == refereeEmail
                && r.PartnerId == partnerIdGuid.ToString()
                && r.Location == locationInfoResponse.Id.ToString())),
            Times.Once);
            _customerProfileClientMock.Verify(x => x.CustomerProfiles.GetByCustomerIdAsync(referrerId, false, false), Times.Exactly(4));
            _customerProfileClientMock.Verify(x => x.CustomerProfiles.GetByCustomerIdAsync(refereeId, false, false), Times.Once);

            Assert.Equal(ReferralInformationStatus.OK, response.Status);
            Assert.Equal(4, response.Referrals.Count);
            Assert.Equal(ReferralStatus.ReferralNotConfirmed, response.Referrals[0].ReferralStatus);
            Assert.Equal($"{referrerFirstName} {referrerLastName}", response.Referrals[0].ReferrerAdditionalInfo);
            Assert.Equal(referrerEmail, response.Referrals[0].ReferrerEmail);
            Assert.Equal(referralHotelId, response.Referrals[0].ReferralId);
            Assert.Equal(ReferralStatus.OK, response.Referrals[1].ReferralStatus);
            Assert.Equal(ReferralStatus.ReferralAlreadyUsed, response.Referrals[2].ReferralStatus);
            Assert.Equal(ReferralStatus.ReferralExpired, response.Referrals[3].ReferralStatus);
        }

        [Fact]
        public async Task When_GetReferralInformationAsync_CalledWithMissingEmail_ExpectExceptionThrown()
        {
            //Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetReferralInformationAsync(new ReferralInformationRequest{CustomerId = null}));
        }

        [Fact]
        public async Task When_GetReferralInformationAsync_CalledWithMissingPartnerId_ExpectExceptionThrown()
        {
            //Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetReferralInformationAsync(new ReferralInformationRequest{CustomerId = "notnull", PartnerId = null}));
        }
    }
}

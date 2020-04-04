using AutoMapper;
using Xunit;

namespace MAVN.Service.PartnersIntegration.Tests
{
    public class AutoMapperProfileTests
    {
        private readonly IMapper _mapper;

        public AutoMapperProfileTests()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DomainServices.AutoMapperProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Fact]
        public void Mapping_Configuration_Is_Correct()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();

            Assert.True(true);
        }
    }
}

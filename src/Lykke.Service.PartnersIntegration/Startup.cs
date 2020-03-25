using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.PartnersIntegration.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using AutoMapper;
using Lykke.Service.PartnersIntegration.DomainServices.CallbackClient;
using Lykke.Service.PartnersIntegration.Profiles;
using Microsoft.Extensions.Http;

namespace Lykke.Service.PartnersIntegration
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "PartnersIntegration API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.ConfigureAll<HttpClientFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(builder =>
                {
                    builder.AdditionalHandlers.Add(new TimeoutHandler());
                });
                options.HttpClientActions.Add(client =>
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                });
            });

            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Extend = (serviceCollection, settings) =>
                {
                    serviceCollection.AddAutoMapper(new Type[]
                    {
                        typeof(AutoMapperProfile),
                        typeof(DomainServices.AutoMapperProfile),
                        typeof(MsSqlRepositories.AutoMapperProfile)
                    });
                };

                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "PartnersIntegrationLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.PartnersIntegrationService.Db.LogsConnString;
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IMapper mapper)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
            });

            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}

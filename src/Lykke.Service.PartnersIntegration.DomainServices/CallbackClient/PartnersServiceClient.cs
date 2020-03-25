using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Service.PartnersIntegration.DomainServices.CallbackClient
{
    public class PartnersServiceClient
    {
        public HttpClient Client { get; }

        public PartnersServiceClient(HttpClient client)
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            Client = client;
        }

        public async Task SendAsync(HttpRequestMessage request)
        {
            await Client.SendAsync(request);
        }
    }
}

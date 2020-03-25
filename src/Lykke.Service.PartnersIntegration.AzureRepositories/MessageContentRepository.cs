using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PartnersIntegration.Domain.Repositories;

namespace Lykke.Service.PartnersIntegration.AzureRepositories
{
    public class MessageContentRepository : IMessageContentRepository
    {
        private readonly IBlobStorage _blobStorage;
        private const string ContainerName = "partnermessages";

        public MessageContentRepository(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task SaveContentAsync(string messageId, string content)
        {
            await _blobStorage.CreateContainerIfNotExistsAsync(ContainerName);

            if (string.IsNullOrEmpty(content))
                return;

            await _blobStorage.SaveBlobAsync(ContainerName, messageId, Encoding.UTF8.GetBytes(content));
        }

        public async Task<string> GetContentAsync(string messageId)
        {
            bool exists = await _blobStorage.HasBlobAsync(ContainerName, messageId);
            if (!exists)
                return null;

            return await _blobStorage.GetAsTextAsync(ContainerName, messageId);
        }

        public async Task DeleteContentAsync(string messageId)
        {
            var blobExists = await _blobStorage.HasBlobAsync(ContainerName, messageId);

            if (blobExists)
                await _blobStorage.DelBlobAsync(ContainerName, messageId);
        }
    }
}

using System.Threading.Tasks;

namespace Lykke.Service.PartnersIntegration.Domain.Repositories
{
    public interface IMessageContentRepository
    {
        Task SaveContentAsync(string messageId, string content);

        Task<string> GetContentAsync(string messageId);

        Task DeleteContentAsync(string messageId);
    }
}

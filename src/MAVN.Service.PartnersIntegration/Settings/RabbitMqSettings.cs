using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PartnersIntegration.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        [AmqpCheck]
        public string WalletConnectionString { get; set; }    
    }
}

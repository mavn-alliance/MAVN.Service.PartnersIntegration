using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PartnersIntegration.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public string DataConnString { get; set; }

        [AzureTableCheck]
        public string MessageContentConnString { get; set; }
    }
}

using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PartnersIntegration.Client 
{
    /// <summary>
    /// PartnersIntegration client settings.
    /// </summary>
    public class PartnersIntegrationServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}

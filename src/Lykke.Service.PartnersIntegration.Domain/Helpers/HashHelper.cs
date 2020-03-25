using System.Security.Cryptography;
using System.Text;

namespace Lykke.Service.PartnersIntegration.Domain.Helpers
{
    public static class HashHelper
    {
        public static string ComputeSha256Hash(string rawData)
        {
            if (rawData == null)
            {
                rawData = string.Empty;
            }

            // Create a SHA256   
            var builder = new StringBuilder();
            using (var sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}

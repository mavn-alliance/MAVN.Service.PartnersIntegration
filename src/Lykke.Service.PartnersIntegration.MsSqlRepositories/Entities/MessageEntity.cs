using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lykke.Service.PartnersIntegration.MsSqlRepositories.Entities
{
    [Table("messages")]
    public class MessageEntity : EntityBase
    {
        public DateTime CreationTimestamp { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartnerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }
        
        [MaxLength(100)]
        public string ExternalLocationId { get; set; }

        [MaxLength(100)]
        public string PosId { get; set; }
    }
}

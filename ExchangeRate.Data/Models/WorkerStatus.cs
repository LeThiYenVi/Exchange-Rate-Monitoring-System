using System.ComponentModel.DataAnnotations;

namespace ExchangeRate.Data.Models
{
    public class WorkerStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime LastHeartbeat { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}

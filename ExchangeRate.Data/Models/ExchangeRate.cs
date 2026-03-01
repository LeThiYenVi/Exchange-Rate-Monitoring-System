using System.ComponentModel.DataAnnotations;

namespace ExchangeRate.Data.Models
{
    public class ExchangeRate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string CurrencyCode { get; set; } = string.Empty;

        [Required]
        public decimal Rate { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}

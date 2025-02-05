using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagerAPI.Domain.Entities {
    public class User {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; } //Email único

        [Required]
        public string? PasswordHash { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}

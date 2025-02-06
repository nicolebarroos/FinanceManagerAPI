using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceManagerAPI.Domain.Entities {
    public class Transaction {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } //Referência ao usuário

        [Required]
        public int CategoryId { get; set; } //Referência à categoria

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; } //Receita ou Despesa

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; } //representa o relacionamento com a entidade Category.

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }

    public enum TransactionType {
        Income,  // Receita
        Expense  // Despesa
    }
}

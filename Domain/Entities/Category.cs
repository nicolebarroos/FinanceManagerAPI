using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagerAPI.Domain.Entities {
    public class Category {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Name { get; set; }

        //indica que uma Categoria pode ter várias Transações associadas (One-to-Many ou 1:N)
        //virtual: Permite que o Entity Framework use Lazy Loading (carregamento sob demanda).
            //técnica onde os dados só são carregados do banco quando realmente são acessados no código.
        //ICollection<Transaction>	Define que a categoria terá uma coleção de transações.
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}

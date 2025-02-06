using System.Security.Claims;
using FinanceManagerAPI.Domain.Entities;
using FinanceManagerAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerAPI.Presentation.Controllers {
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase {
        private readonly ApplicationDbContext _dbContext;

        public TransactionController(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTransaction([FromBody] Transaction transaction) {
            //User → Representa o usuário autenticado na requisição (fornecido pelo ASP.NET Core).
            //FindFirst(ClaimTypes.NameIdentifier) → Busca no token JWT o sub (Subject), que geralmente é o ID do usuário.
            //?.Value → Retorna o valor do Claim (o ID do usuário) ou null se não encontrar.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            //Garantir que a categoria informada existe
            var category = await _dbContext.Categories.FindAsync(transaction.CategoryId);
            if (category == null) return BadRequest("Categoria inválida.");

            transaction.UserId = int.Parse(userId);
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Transação criada com sucesso!", transaction });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTransactions() {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var transactions = await _dbContext.Transactions
                .Where(t => t.UserId == int.Parse(userId))
                .Include(t => t.Category) //Inclui a categoria na resposta
                .ToListAsync();

            return Ok(transactions);
        }

        //Buscar uma transação específica do usuário autenticado
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTransaction(int id) {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var transaction = await _dbContext.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == int.Parse(userId));

            if (transaction == null) return NotFound("Transação não encontrada.");

            return Ok(transaction);
        }

        //Atualizar uma transação existente (somente do usuário autenticado)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] Transaction updatedTransaction) {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var transaction = await _dbContext.Transactions.FindAsync(id);
            if (transaction == null) return NotFound("Transação não encontrada.");
            if (transaction.UserId != int.Parse(userId)) return Forbid();

            //Atualizando os campos
            transaction.Amount = updatedTransaction.Amount;
            transaction.Type = updatedTransaction.Type;
            transaction.Date = updatedTransaction.Date;
            transaction.Description = updatedTransaction.Description;
            transaction.CategoryId = updatedTransaction.CategoryId;

            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Transação atualizada com sucesso!", transaction });
        }

        //Excluir uma transação (somente do usuário autenticado)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTransaction(int id) {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var transaction = await _dbContext.Transactions.FindAsync(id);
            if (transaction == null) return NotFound("Transação não encontrada.");
            if (transaction.UserId != int.Parse(userId)) return Forbid();

            _dbContext.Transactions.Remove(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok("Transação excluída com sucesso.");
        }
    }
}

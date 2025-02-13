using System.Security.Claims;
using FinanceManagerAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerAPI.Presentation.Controllers {
    [ApiController]
    [Route("api/reports")]
    public class ReportController : ControllerBase {
        private readonly ApplicationDbContext _dbContext;

        public ReportController(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        //Retorna o total de receitas e despesas do usuário autenticado
        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetFinancialSummary([FromQuery] int year, [FromQuery] int month) {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Usuário não autenticado.");

            // 🔥 DEBUG: Ver todas as transações no banco
            var allTransactions = await _dbContext.Transactions.ToListAsync();
            Console.WriteLine("===== TODAS AS TRANSAÇÕES NO BANCO =====");
            foreach (var t in allTransactions) {
                Console.WriteLine($"ID: {t.Id}, UserId: {t.UserId}, Amount: {t.Amount}, Type: {t.Type}, Date: {t.Date}");
            }
            Console.WriteLine("========================================");

            // 🔥 DEBUG: Verificar se há despesas antes do filtro
            var expensesCheck = allTransactions
                .Where(t => t.UserId == int.Parse(userId) && t.Type == Domain.Entities.TransactionType.Expense)
                .ToList();

            Console.WriteLine("🔥 Despesas encontradas antes do filtro:");
            foreach (var expense in expensesCheck) {
                Console.WriteLine($"CategoryId: {expense.CategoryId}, Amount: {expense.Amount}");
            }
            Console.WriteLine("========================================");


            var transactions = await _dbContext.Transactions
                .Where(t => t.UserId == int.Parse(userId) && t.Date.Year == year && t.Date.Month == month)
                .ToListAsync();

            var totalIncome = transactions.Where(t => t.Type == Domain.Entities.TransactionType.Income).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Type == Domain.Entities.TransactionType.Expense).Sum(t => t.Amount);
            var balance = totalIncome - totalExpense;

            var result = new {
                totalIncome,
                totalExpense,
                balance
            };

            Console.WriteLine($"Retorno da API: {System.Text.Json.JsonSerializer.Serialize(result)}"); // 🔥 Ver o que está sendo retornado!

            return Ok(result);
        }

        //Retorna o total de despesas por categoria
        [HttpGet("by-category")]
        [Authorize]
        public async Task<IActionResult> GetExpensesByCategory([FromQuery] int year, [FromQuery] int month) {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Usuário não autenticado.");


            var transactions = await _dbContext.Transactions
                .Include(t => t.Category) // 🔥 Garante que a Categoria está carregada
                .Where(t => t.UserId == int.Parse(userId) && t.Date.Year == year && t.Date.Month == month && t.Type == Domain.Entities.TransactionType.Expense)
                .GroupBy(t => t.Category != null ? t.Category.Name : "Sem Categoria") 
                .Select(g => new {
                    Category = g.Key,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            Console.WriteLine($"🔥 Retorno da API - Despesas por Categoria (corrigido): {System.Text.Json.JsonSerializer.Serialize(transactions)}");


            return Ok(transactions);
        }
    }
}

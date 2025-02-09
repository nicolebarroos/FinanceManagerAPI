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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var transactions = await _dbContext.Transactions
                .Where(t => t.UserId == int.Parse(userId) && t.Date.Year == year && t.Date.Month == month)
                .ToListAsync();

            var totalIncome = transactions.Where(t => t.Type == Domain.Entities.TransactionType.Income).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Type == Domain.Entities.TransactionType.Expense).Sum(t => t.Amount);
            var balance = totalIncome - totalExpense;

            return Ok(new {
                totalIncome,
                totalExpense,
                balance
            });
        }

        //Retorna o total de despesas por categoria
        [HttpGet("by-category")]
        [Authorize]
        public async Task<IActionResult> GetExpensesByCategory([FromQuery] int year, [FromQuery] int month) {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var transactions = await _dbContext.Transactions
                .Where(t => t.UserId == int.Parse(userId) && t.Date.Year == year && t.Date.Month == month && t.Type == Domain.Entities.TransactionType.Expense)
                .GroupBy(t => t.Category.Name)
                .Select(g => new {
                    Category = g.Key,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            return Ok(transactions);
        }
    }
}

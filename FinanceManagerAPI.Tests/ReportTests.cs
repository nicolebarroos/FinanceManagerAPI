using System.Security.Claims;
using System.Threading.Tasks;
using FinanceManagerAPI.Domain.Entities;
using FinanceManagerAPI.Infrastructure.Persistence;
using FinanceManagerAPI.Presentation.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace FinanceManagerAPI.Tests {
    public class ReportControllerTests : IAsyncLifetime {
        private readonly ApplicationDbContext _dbContext;
        private readonly ReportController _controller;
        private readonly string _dbName;
        
        public ReportControllerTests() {
            _dbName = $"TestDb_{Guid.NewGuid()}"; //Nome único para cada execução
            _dbContext = GetDbContext();
            _controller = GetControllerWithUser(_dbContext);
        }

        private ApplicationDbContext GetDbContext() {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_dbName) //Cada teste usa um banco separado
                .Options;
            return new ApplicationDbContext(options);
        }

        private ReportController GetControllerWithUser(ApplicationDbContext dbContext) {
            var controller = new ReportController(dbContext);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            controller.ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        public async Task InitializeAsync() {
            _dbContext.Database.EnsureCreated();

            _dbContext.Categories.AddRange(new List<Category>
            {
                new Category { Id = 1, Name = "Alimentação" },
                new Category { Id = 2, Name = "Transporte" }
            });
            await _dbContext.SaveChangesAsync();

            _dbContext.Transactions.AddRange(new List<Transaction>
            {
                new Transaction { Amount = 1000, Type = TransactionType.Income, CategoryId = 1, UserId = 1, Date = new DateTime(2025, 2, 1) },
                new Transaction { Amount = 300, Type = TransactionType.Expense, CategoryId = 1, UserId = 1, Date = new DateTime(2025, 2, 2) },
                new Transaction { Amount = 150, Type = TransactionType.Expense, CategoryId = 2, UserId = 1, Date = new DateTime(2025, 2, 3) }
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync() {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }

        [Fact]
        public async Task GetFinancialSummary_Should_Return_Correct_Summary() {
            var result = await _controller.GetFinancialSummary(2025, 2);
            var okResult = result as OkObjectResult;
            var summary = okResult?.Value;

            Assert.NotNull(summary);
            Assert.Equal(1000, (decimal)summary.GetType().GetProperty("totalIncome").GetValue(summary));
            Assert.Equal(450, (decimal)summary.GetType().GetProperty("totalExpense").GetValue(summary));
            Assert.Equal(550, (decimal)summary.GetType().GetProperty("balance").GetValue(summary));
        }

        [Fact]
        public async Task GetExpensesByCategory_Should_Return_Correct_Totals() {
            var result = await _controller.GetExpensesByCategory(2025, 2);
            var okResult = result as OkObjectResult;
            var categories = okResult?.Value as IEnumerable<object>;

            Assert.NotNull(categories);
            var categoryList = categories
                .Select(c => c.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(c)))
                .ToList();

            Assert.Equal(2, categoryList.Count);
            var foodCategory = categoryList.FirstOrDefault(c => c["Category"].ToString() == "Alimentação");
            var transportCategory = categoryList.FirstOrDefault(c => c["Category"].ToString() == "Transporte");

            Assert.NotNull(foodCategory);
            Assert.NotNull(transportCategory);

            Assert.Equal(300, Convert.ToDecimal(foodCategory["TotalAmount"]));
            Assert.Equal(150, Convert.ToDecimal(transportCategory["TotalAmount"]));
        }

        [Fact]
        public async Task GetFinancialSummary_Should_Return_Unauthorized_If_User_Not_Authenticated() {
            var controller = new ReportController(_dbContext);
            var result = await controller.GetFinancialSummary(2025, 2);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task GetExpensesByCategory_Should_Return_Unauthorized_If_User_Not_Authenticated() {
            var controller = new ReportController(_dbContext);
            var result = await controller.GetExpensesByCategory(2025, 2);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }
    }
}

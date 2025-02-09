using FinanceManagerAPI.Domain.Entities;
using FinanceManagerAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerAPI.Presentation.Controllers {
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase {
        private readonly ApplicationDbContext _dbContext;

        public CategoryController(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        //Criar uma nova categoria
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCategory([FromBody] Category category) {
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Categoria criada com sucesso!", category });
        }

        //Listar todas as categorias
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCategories() {
            var categories = await _dbContext.Categories.ToListAsync();
            return Ok(categories);
        }
    }
}

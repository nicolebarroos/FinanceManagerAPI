using FinanceManagerAPI.Domain.Entities;
using FinanceManagerAPI.Infrastructure.Persistence;
using FinanceManagerAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FinanceManagerAPI.Presentation.Controllers {
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase {
        private readonly ApplicationDbContext _dbContext;
        private readonly AuthService _authService;

        public AuthController(ApplicationDbContext dbContext, AuthService authService) {
            _dbContext = dbContext;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user) {
            if (await _dbContext.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest("E-mail já cadastrado.");

            user.PasswordHash = HashPassword(user.PasswordHash);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok("Usuário registrado com sucesso.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user) {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser == null || existingUser.PasswordHash != HashPassword(user.PasswordHash))
                return Unauthorized("Credenciais inválidas.");

            var token = _authService.GenerateJwtToken(existingUser);
            return Ok(new { Token = token });
        }

        private string HashPassword(string password) {
            using (var sha256 = SHA256.Create()) {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}

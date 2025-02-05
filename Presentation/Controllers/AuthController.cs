using FinanceManagerAPI.Domain.Entities;
using FinanceManagerAPI.Infrastructure.Persistence;
using FinanceManagerAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

// Define o espaço onde este arquivo pertence.
namespace FinanceManagerAPI.Presentation.Controllers {
    //Indica que este é um Controller de API, validando automaticamente os inputs.
    [ApiController]
    //Todos os endpoints deste controller estarão sob /api/auth
    [Route("api/auth")]
    //Cria um Controller API (sem suporte a views, apenas JSON).
    public class AuthController : ControllerBase {
        //Permite acessar o banco de dados.
        private readonly ApplicationDbContext _dbContext;
        //Permite gerar tokens JWT.
        private readonly AuthService _authService;

        //Recebe as dependências pelo construtor (injeção de dependência).
        public AuthController(ApplicationDbContext dbContext, AuthService authService) {
            _dbContext = dbContext;
            _authService = authService;
        }

        //Task<> → Indica que o método é assíncrono (async).
        //IActionResult → Define diferentes tipos de resposta HTTP (200 OK, 404 NotFound, etc.).
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user) {
            //Verifica se já existe um usuário com o mesmo e-mail no banco.
            if (await _dbContext.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest("E-mail já cadastrado.");
            //criptografar a senha antes de salvar no banco.
            user.PasswordHash = HashPassword(user.PasswordHash);
            //Adiciona o usuário ao banco.
            _dbContext.Users.Add(user);
            // Salva as mudanças de forma assíncrona.
            await _dbContext.SaveChangesAsync();

            return Ok("Usuário registrado com sucesso.");
        }

        [HttpPost("login")]
        //Recebe um objeto User no corpo da requisição
        public async Task<IActionResult> Login([FromBody] User user) {
            //Procura no banco um usuário com o e-mail informado
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            //Se o e-mail não existir ou a senha estiver errada, retorna 401 Unauthorized.
            if (existingUser == null || existingUser.PasswordHash != HashPassword(user.PasswordHash))
                return Unauthorized("Credenciais inválidas.");
            //Se o login for válido, gera um token JWT e retorna no JSON.
            var token = _authService.GenerateJwtToken(existingUser);
            return Ok(new { Token = token });
        }

        //Converte a senha para um array de bytes (Encoding.UTF8.GetBytes(password)).
        //Aplica o algoritmo SHA-256 (sha256.ComputeHash()).
        //Transforma os bytes em uma string hexadecimal (BitConverter.ToString()).
        //Retorna a senha criptografada
        private string HashPassword(string password) {
            using (var sha256 = SHA256.Create()) {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}

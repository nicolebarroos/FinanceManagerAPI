using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerAPI.Presentation.Controllers {
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase {
        [HttpGet("profile")]
        [Authorize] //Protege a rota com JWT
        public IActionResult GetUserProfile() {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Usuário não encontrado no token.");

            return Ok(new { message = "Perfil do usuário acessado com sucesso!", userId });
        }
    }
}

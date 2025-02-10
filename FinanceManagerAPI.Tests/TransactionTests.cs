//Permite simular um usuário autenticado.
using System.Security.Claims;
//Permite rodar código assíncrono (async/await).
using System.Threading.Tasks;
using FinanceManagerAPI.Domain.Entities;
using FinanceManagerAPI.Infrastructure.Persistence;
using FinanceManagerAPI.Presentation.Controllers;

//Permite simular requisições HTTP.
using Microsoft.AspNetCore.Http;
//Permite trabalhar com respostas da API (OkObjectResult, BadRequest, etc.).
using Microsoft.AspNetCore.Mvc;
//Permite criar e manipular um banco de dados em memória.
using Microsoft.EntityFrameworkCore;
//Importa o framework de testes xUnit
using Xunit;

//Define o "grupo" do código. Aqui, todos os testes estarão dentro de FinanceManagerAPI.Tests.
namespace FinanceManagerAPI.Tests {
    //Cria uma "caixa" chamada TransactionControllerTests, onde vamos colocar nossos testes.
    public class TransactionControllerTests {
        //Cria um banco de dados falso (TestDb) para os testes rodarem sem precisar de um banco real.
        private ApplicationDbContext GetDbContext() {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                //Usa UseInMemoryDatabase, que significa "Banco de Dados em Memória". Isso evita usar um banco físico (PostgreSQL, MySQL, etc.).
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            //Retorna um objeto ApplicationDbContext, que representa o banco no código.
            return new ApplicationDbContext(options);
        }

        /// <summary>
        /// Retorna um `TransactionController` configurado com um usuário autenticado.
        /// </summary>
        //Cria o TransactionController, que é o código que vamos testar.

        //private → A função só pode ser usada dentro da classe de teste.
        //TransactionController → O tipo de retorno da função (ela retorna um TransactionController pronto para uso).
        //GetControllerWithUser → Nome da função.
        //(ApplicationDbContext dbContext) → Recebe um banco de dados falso (dbContext) como argumento.
        private TransactionController GetControllerWithUser(ApplicationDbContext dbContext) {
            var controller = new TransactionController(dbContext);

            //Cria um usuário falso (ClaimsPrincipal), que tem um ID "1".
            //O ClaimTypes.NameIdentifier é onde o ASP.NET guarda o ID do usuário autenticado.
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            //Associa esse usuário ao ControllerContext, simulando um usuário autenticado.
            controller.ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }
        //O atributo [Fact] indica um método de teste que é executado pelo executor de teste
        [Fact]
        public async Task CreateTransaction_Should_Return_Success() {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetControllerWithUser(dbContext);

            // Adicionando uma categoria válida para garantir que ela exista no banco
            dbContext.Categories.Add(new Category { Id = 1, Name = "Alimentação" });
            await dbContext.SaveChangesAsync();

            var transaction = new Transaction {
                Amount = 100,
                Type = TransactionType.Expense,
                CategoryId = 1
            };

            //Executando o Código (Act)
            var result = await controller.CreateTransaction(transaction);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}

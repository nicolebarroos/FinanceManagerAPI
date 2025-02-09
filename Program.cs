using FinanceManagerAPI.Infrastructure.Services;
using FinanceManagerAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json.Serialization;
using FinanceManagerAPI.Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);

//Adicionando conexão com o banco de dados
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Adicionando serviço de autenticação JWT
builder.Services.AddScoped<AuthService>();

//Configuração da autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["JwtSettings:Issuer"],
            ValidAudience = config["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Secret"]))
        };
    });

builder.Services.ConfigureJsonSerialization();

builder.Services.AddAuthorization();

//Adicionando suporte a Controllers
builder.Services.AddControllers(); 

var app = builder.Build();

//Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

//Verificação para garantir que a API iniciou corretamente
Console.WriteLine("🚀 API FinanceManager está rodando!");

//Configurar rotas do controlador
app.MapControllers();

app.Run();

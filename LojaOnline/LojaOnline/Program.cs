using Microsoft.EntityFrameworkCore;
using LojaOnline.Data;
using LojaOnline.Services;
using LojaOnline.MockServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Polly;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// --- Configuração CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()  // Permite qualquer origem (para desenvolvimento)
              .AllowAnyMethod()  // Permite GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader(); // Permite qualquer header
    });
});
// --- Fim Configuração CORS ---

builder.Services.AddControllers();

// Registar AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// --- Configurar Redis Cache ---
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
// --- Fim Configuração Redis ---

// --- Configurar HttpClient para Pagamentos ---
builder.Services.AddHttpClient<IExternalPaymentService, ExternalPaymentService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:PaymentApiUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});
// --- Fim Configuração HttpClient ---

// Configurar Autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Iniciar WireMock para simular serviços externos
    WireMockSetup.Start();
}

// Comentado para desenvolvimento - permite usar apenas HTTP
// app.UseHttpsRedirection();

// --- Ativar CORS (IMPORTANTE: deve vir antes de UseAuthorization) ---
app.UseCors("AllowFrontend");
// --- Fim Ativar CORS ---

// --- AUTO-MIGRATION (Dev Trick) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    try
    {
        // Tenta adicionar a coluna Size se não existir
        // Em MySQL, "IF NOT EXISTS" para colunas pode ser complexo, 
        // mas podemos tentar executar e ignorar erro se já existir.
        // Ou verificar schemas. Simplificando:
        db.Database.ExecuteSqlRaw("ALTER TABLE OrderItems ADD COLUMN Size VARCHAR(50) DEFAULT '';");
    }
    catch
    {
        // Ignora erro se a coluna já existir
    }
}
// --- FIM AUTO-MIGRATION ---

app.UseAuthentication(); // Adicionar antes de Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();

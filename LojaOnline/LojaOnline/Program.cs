using Microsoft.EntityFrameworkCore;
using LojaOnline.Data;

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
}

// Comentado para desenvolvimento - permite usar apenas HTTP
// app.UseHttpsRedirection();

// --- Ativar CORS (IMPORTANTE: deve vir antes de UseAuthorization) ---
app.UseCors("AllowFrontend");
// --- Fim Ativar CORS ---

app.UseAuthorization();

app.MapControllers();

app.Run();

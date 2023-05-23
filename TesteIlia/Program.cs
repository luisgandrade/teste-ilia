using TesteIlia.Persistencia.Repositorio;
using TesteIlia.Servicos.Ponto;
using TesteIlia.Servicos.RelatorioDePonto;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IRegistroDeBatidaRepositorio, RegistroDeBatidaRepositorioMemoria>();
builder.Services.AddScoped<IBatedorDePonto, BatedorDePonto>();
builder.Services.AddScoped<IGeradorDeRelatorioDePonto, GeradorDeRelatorioDePonto>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

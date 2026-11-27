using System.Text.Json.Serialization;
using Granja.Infrastructure;
using Granja.Infrastructure.Data;
using Granja.Services;
using Granja.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Raíz de composición
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Default") ?? "Data Source=granja.db");
builder.Services.AddServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InicializarAsync(scope.ServiceProvider.GetRequiredService<GranjaDbContext>());
}

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

app.Run();

public partial class Program { }

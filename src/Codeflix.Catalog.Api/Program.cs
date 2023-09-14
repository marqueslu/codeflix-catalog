using Codeflix.Catalog.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddAppConnections()
    .AddUseCases()
    .AddAndConfigureControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseDocumentation();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}
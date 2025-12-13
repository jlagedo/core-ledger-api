using Microsoft.EntityFrameworkCore;
using coreledger.model;
using core_ledger_api.Infra;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    options.UseInMemoryDatabase("ToDos");
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();

app.Run();

using Microsoft.EntityFrameworkCore;
using coreledger.model;
using core_ledger_api.Infra;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();

app.Run();

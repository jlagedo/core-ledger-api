using Microsoft.EntityFrameworkCore;
using coreledger.model;

namespace core_ledger_api.Infra
{
    public class ToDoDbContext : DbContext
    {
        public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
        {
        }

        public DbSet<ToDo> ToDos => Set<ToDo>();
    }

}


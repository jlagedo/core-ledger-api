using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using coreledger.model;
using core_ledger_api.Infra;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ToDoDbContext _context;

    public TodosController(ToDoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IResult> GetAll()
    {
        var todos = await _context.ToDos.ToListAsync();
        return TypedResults.Ok(todos);
    }

    [HttpGet("{id}", Name = "GetTodoById")]
    public async Task<IResult> GetById(int id)
    {
        var todo = await _context.ToDos.FindAsync(id);
        
        if (todo == null)
        {
            return TypedResults.NotFound();
        }
        
        return TypedResults.Ok(todo);
    }

    [HttpPost]
    public async Task<IResult> Create(ToDo todo)
    {
        _context.ToDos.Add(todo);
        await _context.SaveChangesAsync();

        return TypedResults.CreatedAtRoute(todo, "GetTodoById", new { id = todo.Id });
    }

    [HttpPut("{id}")]
    public async Task<IResult> Update(int id, ToDo inputTodo)
    {
        var todo = await _context.ToDos.FindAsync(id);

        if (todo == null)
        {
            return TypedResults.NotFound();
        }

        todo.Description = inputTodo.Description;
        todo.IsCompleted = inputTodo.IsCompleted;

        await _context.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IResult> Delete(int id)
    {
        var todo = await _context.ToDos.FindAsync(id);
        
        if (todo == null)
        {
            return TypedResults.NotFound();
        }

        _context.ToDos.Remove(todo);
        await _context.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}

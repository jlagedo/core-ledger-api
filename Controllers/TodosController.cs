using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using coreledger.model;
using core_ledger_api.Infra;
using AutoMapper;
using core_ledger_api.Dtos;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ToDoDbContext _context;
    private readonly IMapper _mapper;

    public TodosController(ToDoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IResult> GetAll()
    {
        var todos = await _context.ToDos.AsNoTracking().ToListAsync();
        var todoDtos = _mapper.Map<List<ToDoDto>>(todos);
        return TypedResults.Ok(todoDtos);
    }

    [HttpGet("{id}", Name = "GetTodoById")]
    public async Task<IResult> GetById(int id)
    {
        var todo = await _context.ToDos.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        
        if (todo == null)
        {
            return TypedResults.NotFound();
        }
        
        var todoDto = _mapper.Map<ToDoDto>(todo);
        return TypedResults.Ok(todoDto);
    }

    [HttpPost]
    public async Task<IResult> Create(CreateToDoDto createToDoDto)
    {
        var todo = _mapper.Map<ToDo>(createToDoDto);
        _context.ToDos.Add(todo);
        await _context.SaveChangesAsync();

        var todoDto = _mapper.Map<ToDoDto>(todo);
        return TypedResults.CreatedAtRoute(todoDto, "GetTodoById", new { id = todoDto.Id });
    }

    [HttpPut("{id}")]
    public async Task<IResult> Update(int id, UpdateToDoDto updateToDoDto)
    {
        var todo = await _context.ToDos.FindAsync(id);

        if (todo == null)
        {
            return TypedResults.NotFound();
        }

        _mapper.Map(updateToDoDto, todo);

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

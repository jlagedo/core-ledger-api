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
    private readonly ILogger<TodosController> _logger;

    public TodosController(
        ToDoDbContext context, 
        IMapper mapper,
        ILogger<TodosController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IResult> GetAll()
    {
        _logger.LogInformation("Retrieving all ToDo items");
        
        var todos = await _context.ToDos.AsNoTracking().ToListAsync();
        var todoDtos = _mapper.Map<List<ToDoDto>>(todos);
        
        _logger.LogInformation("Retrieved {Count} ToDo items", todoDtos.Count);
        
        return TypedResults.Ok(todoDtos);
    }

    [HttpGet("{id}", Name = "GetTodoById")]
    public async Task<IResult> GetById(int id)
    {
        _logger.LogInformation("Retrieving ToDo item {TodoId}", id);
        
        var todo = await _context.ToDos.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        
        if (todo == null)
        {
            _logger.LogWarning("ToDo item {TodoId} not found", id);
            return TypedResults.NotFound();
        }
        
        var todoDto = _mapper.Map<ToDoDto>(todo);
        return TypedResults.Ok(todoDto);
    }

    [HttpPost]
    public async Task<IResult> Create(CreateToDoDto createToDoDto)
    {
        _logger.LogInformation("Creating new ToDo item with description: {Description}", 
            createToDoDto.Description);
        
        var todo = _mapper.Map<ToDo>(createToDoDto);
        _context.ToDos.Add(todo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created ToDo item {TodoId}", todo.Id);
        
        var todoDto = _mapper.Map<ToDoDto>(todo);
        return TypedResults.CreatedAtRoute(todoDto, "GetTodoById", new { id = todoDto.Id });
    }

    [HttpPut("{id}")]
    public async Task<IResult> Update(int id, UpdateToDoDto updateToDoDto)
    {
        _logger.LogInformation("Updating ToDo item {TodoId}", id);
        
        var todo = await _context.ToDos.FindAsync(id);

        if (todo == null)
        {
            _logger.LogWarning("ToDo item {TodoId} not found for update", id);
            return TypedResults.NotFound();
        }

        _mapper.Map(updateToDoDto, todo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated ToDo item {TodoId}", id);
        
        return TypedResults.NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IResult> Delete(int id)
    {
        _logger.LogInformation("Deleting ToDo item {TodoId}", id);
        
        var todo = await _context.ToDos.FindAsync(id);
        
        if (todo == null)
        {
            _logger.LogWarning("ToDo item {TodoId} not found for deletion", id);
            return TypedResults.NotFound();
        }

        _context.ToDos.Remove(todo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted ToDo item {TodoId}", id);
        
        return TypedResults.NoContent();
    }
}

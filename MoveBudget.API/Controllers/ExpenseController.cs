using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoveBudget.API.Data;
using MoveBudget.API.Models;
using MoveBudget.API.Services;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExpenseController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retorna a lista de despesas registradas com filtros opcionais e ordenação.
    /// </summary>
    /// <param name="filters">Filtros opcionais: categoria, moeda, datas e ordenação.</param>
    /// <returns>Lista de despesas filtradas.</returns>
    /// <response code="200">Retorna a lista de despesas.</response>

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExpenseDTO>), StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDTO>>> Get([FromQuery] ExpenseFilterDTO filters)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        IQueryable<Expense> query = _context.Expenses.AsNoTracking()
                                                    .Where(e => e.UserId == userId);

        if (!string.IsNullOrWhiteSpace(filters.Category))
            query = query.Where(e => e.Category.ToLower() == filters.Category.ToLower());

        if (!string.IsNullOrWhiteSpace(filters.Currency))
            query = query.Where(e => e.Currency.ToLower() == filters.Currency.ToLower());

        if (filters.StartDate.HasValue)
            query = query.Where(e => e.Date >= filters.StartDate.Value);

        if (filters.EndDate.HasValue)
            query = query.Where(e => e.Date <= filters.EndDate.Value);

        query = filters.SortBy?.ToLower() switch
        {
            "value" => filters.Order?.ToLower() == "desc" ? query.OrderByDescending(e => e.Value) : query.OrderBy(e => e.Value),
            "date" => filters.Order?.ToLower() == "desc" ? query.OrderByDescending(e => e.Date) : query.OrderBy(e => e.Date),
            _ => query
        };

        var expenses = await query
            .Select(e => new ExpenseDTO
            {
                Id = e.Id,
                Description = e.Description,
                Category = e.Category,
                Value = e.Value,
                Currency = e.Currency,
                Date = e.Date,
                UserId = e.UserId
            })
            .ToListAsync();

        if (expenses.Count <= 0)
            return NotFound(ApiResponse<ExpenseDTO>.Fail("Despesa não encontrada"));

        return Ok(expenses);
    }


    /// <summary>
    /// Retorna uma despesa específica pelo ID.
    /// </summary>
    /// <param name="id">ID da despesa.</param>
    /// <returns>Despesa correspondente ao ID informado.</returns>
    /// <response code="200">Despesa encontrada.</response>
    /// <response code="404">Despesa não encontrada.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ExpenseDTO>>> GetById(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var expense = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Id == id)
            .Select(e => new ExpenseDTO
            {
                Id = e.Id,
                Description = e.Description,
                Category = e.Category,
                Value = e.Value,
                Currency = e.Currency,
                Date = e.Date,
                UserId = e.UserId
            })
            .FirstOrDefaultAsync();

        if (expense == null)
            return NotFound(ApiResponse<ExpenseDTO>.Fail("Despesa não encontrada"));

        return Ok(ApiResponse<ExpenseDTO>.Ok(expense));
    }


    /// <summary>
    /// Cria uma nova despesa.
    /// </summary>
    /// <param name="dto">Dados da despesa a ser criada.</param>
    /// <returns>Despesa criada.</returns>
    /// <response code="201">Despesa criada com sucesso.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseDTO), StatusCodes.Status201Created)]
    public async Task<ActionResult<ExpenseDTO>> Create([FromBody] CreateExpenseDTO dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var expense = new Expense
        {
            Description = dto.Description,
            Category = dto.Category,
            Value = dto.Value,
            Currency = dto.Currency,
            Date = dto.Date,
            UserId = userId
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var expenseDto = new ExpenseDTO
        {
            Id = expense.Id,
            Description = expense.Description,
            Category = expense.Category,
            Value = expense.Value,
            Currency = expense.Currency,
            Date = expense.Date,
            UserId = userId
        };

        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expenseDto);
    }

    /// <summary>
    /// Atualiza uma despesa existente pelo ID.
    /// </summary>
    /// <param name="id">ID da despesa a ser atualizada.</param>
    /// <param name="dto">Novos dados da despesa.</param>
    /// <returns>Despesa atualizada.</returns>
    /// <response code="200">Despesa atualizada com sucesso.</response>
    /// <response code="404">Despesa não encontrada.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ExpenseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateExpenseDTO dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (expense == null)
            return NotFound(ApiResponse<string>.Fail("Despesa não encontrada")); ;

        expense.Description = dto.Description;
        expense.Category = dto.Category;
        expense.Value = dto.Value;
        expense.Currency = dto.Currency;
        expense.Date = dto.Date;

        await _context.SaveChangesAsync();

        var expenseDto = new ExpenseDTO
        {
            Id = expense.Id,
            Description = expense.Description,
            Category = expense.Category,
            Value = expense.Value,
            Currency = expense.Currency,
            Date = expense.Date,
            UserId = userId
        };

        return Ok(expenseDto);
    }

    /// <summary>
    /// Remove uma despesa pelo ID.
    /// </summary>
    /// <param name="id">ID da despesa a ser removida.</param>
    /// <response code="204">Despesa removida com sucesso.</response>
    /// <response code="404">Despesa não encontrada.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var expense = _context.Expenses.FirstOrDefault(e => e.Id == id && e.UserId == userId);

        if (expense == null)
            return NotFound(ApiResponse<string>.Fail("Despesa não encontrada"));

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Converte um valor entre duas moedas usando a API externa.
    /// </summary>
    /// <param name="from">Moeda de origem (ex: USD).</param>
    /// <param name="to">Moeda de destino (ex: EUR).</param>
    /// <param name="amount">Valor a ser convertido.</param>
    /// <returns>Valor convertido na moeda de destino.</returns>
    /// <response code="200">Conversão realizada com sucesso.</response>
    /// <response code="400">Erro ao realizar a conversão.</response>
    [HttpGet("convert")]
    public async Task<ActionResult<ApiResponse<decimal>>> ConvertCurrency(
        [FromServices] CurrencyConversionService conversionService,
        string from, string to, decimal amount)
    {
        var result = await conversionService.ConvertAsync(from, to, amount);
        if (result == null)
            return BadRequest(ApiResponse<decimal>.Fail("Não foi possível realizar a conversão."));

        return Ok(ApiResponse<decimal>.Ok(result.Value));
    }
}
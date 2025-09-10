using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoveBudget.API.Data;
using MoveBudget.API.Models;
using System.Security.Claims;

namespace MoveBudget.Tests
{
    public class ExpenseControllerTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // DB isolado por teste
                .Options;

            return new AppDbContext(options);
        }

        private ExpenseController GetControllerWithUser(AppDbContext context, int userId = 1)
        {
            var controller = new ExpenseController(context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task Get_WhenNoExpenses_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = GetControllerWithUser(context);

            var result = await controller.Get(new ExpenseFilterDTO());

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task Get_WithCategoryFilter_ReturnsFilteredResults()
        {
            var context = GetDbContext();
            context.Expenses.AddRange(
                new Expense { Id = 1, Description = "Café", Category = "Alimentação", Value = 10, Currency = "BRL", Date = DateTime.UtcNow, UserId = 1 },
                new Expense { Id = 2, Description = "Ônibus", Category = "Transporte", Value = 5, Currency = "BRL", Date = DateTime.UtcNow, UserId = 1 }
            );
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context);

            var filters = new ExpenseFilterDTO { Category = "Alimentação" };
            var result = await controller.Get(filters);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var expenses = Assert.IsAssignableFrom<IEnumerable<ExpenseDTO>>(okResult.Value);

            Assert.Single(expenses);
            Assert.Equal("Café", expenses.First().Description);
        }

        [Fact]
        public async Task GetById_WhenExpenseExists_ReturnsExpense()
        {
            var context = GetDbContext();
            var expense = new Expense { Id = 1, Description = "Livro", Category = "Educação", Value = 50, Currency = "BRL", Date = DateTime.UtcNow, UserId = 1 };
            context.Expenses.Add(expense);
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context);

            var result = await controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<ApiResponse<ExpenseDTO>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal("Livro", response.Data.Description);
        }

        [Fact]
        public async Task GetById_WhenExpenseDoesNotExist_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = GetControllerWithUser(context);

            var result = await controller.GetById(99);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ValidExpense_ReturnsCreatedExpense()
        {
            var context = GetDbContext();
            var controller = GetControllerWithUser(context);

            var dto = new CreateExpenseDTO
            {
                Description = "Café",
                Category = "Alimentação",
                Value = 10,
                Currency = "BRL",
                Date = DateTime.UtcNow
            };

            var result = await controller.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var expense = Assert.IsType<ExpenseDTO>(createdResult.Value);

            Assert.Equal("Café", expense.Description);
        }

        [Fact]
        public async Task Update_WhenExpenseExists_UpdatesExpense()
        {
            var context = GetDbContext();
            var expense = new Expense { Id = 1, Description = "Antigo", Category = "Velho", Value = 20, Currency = "BRL", Date = DateTime.UtcNow, UserId = 1 };
            context.Expenses.Add(expense);
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context);

            var dto = new CreateExpenseDTO
            {
                Description = "Novo",
                Category = "Atualizado",
                Value = 30,
                Currency = "USD",
                Date = DateTime.UtcNow
            };

            var result = await controller.Update(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedExpense = Assert.IsType<ExpenseDTO>(okResult.Value);

            Assert.Equal("Novo", updatedExpense.Description);
            Assert.Equal("USD", updatedExpense.Currency);
        }

        [Fact]
        public async Task Update_WhenExpenseDoesNotExist_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = GetControllerWithUser(context);

            var dto = new CreateExpenseDTO
            {
                Description = "Inexistente",
                Category = "Nada",
                Value = 100,
                Currency = "BRL",
                Date = DateTime.UtcNow
            };

            var result = await controller.Update(99, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenExpenseExists_RemovesExpense()
        {
            var context = GetDbContext();
            var expense = new Expense { Id = 1, Description = "Transporte", Category = "Mobilidade", Value = 20, Currency = "BRL", Date = DateTime.UtcNow, UserId = 1 };
            context.Expenses.Add(expense);
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context);

            var result = await controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Expenses);
        }

        [Fact]
        public async Task Delete_WhenExpenseDoesNotExist_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = GetControllerWithUser(context);

            var result = await controller.Delete(42);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}

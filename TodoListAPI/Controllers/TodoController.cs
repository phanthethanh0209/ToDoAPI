using Microsoft.AspNetCore.Mvc;
using TodoListAPI.Attributes;
using TodoListAPI.DTOs;
using TodoListAPI.Services;

namespace TodoListAPI.Controllers
{
    // C1: [ServiceFilter(typeof(JwtAuthorizeFilter))] // Nếu JwtAuthorizeFilter có constructor nhận service từ DI, ta dùng ServiceFilter.
    [JwtAuthorizeAttribute] // C2
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodoItem([FromBody] TodoDTO todoItem)
        {
            TodoItemResponseDTO? todo = await _todoService.CreateTodoItem(todoItem);
            if (todo == null)
            {
                return BadRequest("Create to do item fail");
            }

            return Ok(todo);
        }

        [HttpGet("{todoId}")]
        public async Task<IActionResult> GetTodoItem(int todoId)
        {
            TodoItemResponseDTO? todo = await _todoService.GetTodoItem(todoId);
            if (todo == null)
            {
                return NotFound();
            }
            return Ok(todo);
        }

        [HttpPut("{todoId}")]
        public async Task<IActionResult> UpdateTodoItem(int todoId, TodoDTO todo)
        {
            TodoItemResponseDTO? todoItem = await _todoService.UpdateTodoItem(todoId, todo);
            if (todoItem == null)
            {
                return BadRequest("Update to do item fail");
            }

            return Ok(todoItem);
        }

        [HttpDelete("todos/{todoId}")]
        public async Task<IActionResult> DeleteTodoItem(int todoId)
        {
            bool todoItem = await _todoService.DeleteTodoItem(todoId);
            if (!todoItem)
            {
                return BadRequest("Delete to do item fail");
            }

            return NoContent();
        }

        [HttpGet("todos")]
        public async Task<IActionResult> GetAllTodo(int page = 1, int limit = 10)
        {
            TodoListDTO? todos = await _todoService.GetAllTodo(page, limit);
            if (todos == null)
            {
                return BadRequest("Delete to do item fail");
            }
            return Ok(todos);
        }

        [HttpGet("todos/filter")]
        public async Task<IActionResult> GetTodoByTitle(string title, int page, int limit)
        {
            TodoListDTO? todos = await _todoService.GetAllTodoByTitle(title, page, limit);
            if (todos == null)
                return NotFound("Not found to do with '" + title + "'");
            return Ok(todos);
        }
    }
}

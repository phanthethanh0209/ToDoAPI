using Microsoft.AspNetCore.Mvc;
using TodoListAPI.DTOs;
using TodoListAPI.Services;

namespace TodoListAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTodoItem([FromBody] TodoDTO todoItem)
        {
            (bool Success, int StatusCode, ErrorMessageDTO Message, TodoItemResponseDTO TodoResponse) todo = await _todoService.CreateTodoItem(todoItem);
            if (!todo.Success)
            {
                return StatusCode(todo.StatusCode, todo.Message);
            }

            return StatusCode(todo.StatusCode, todo.TodoResponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetTodoItem(int id)
        {
            TodoItemResponseDTO todo = await _todoService.GetTodoItem(id);
            if (todo == null)
            {
                return NotFound();
            }
            return Ok(todo);
        }

        //[Authorize]
        [HttpPut("{todoId}")]
        public async Task<IActionResult> UpdateTodoItem(int todoId, TodoDTO todo)
        {
            (bool Success, int StatusCode, ErrorMessageDTO Message, TodoItemResponseDTO TodoResponse) todoItem = await _todoService.UpdateTodoItem(todoId, todo);
            if (!todoItem.Success)
            {
                return StatusCode(todoItem.StatusCode, todoItem.Message);
            }

            return StatusCode(todoItem.StatusCode, todoItem.TodoResponse);
        }

        [HttpDelete("{todoId}")]
        public async Task<IActionResult> DeleteTodoItem(int todoId)
        {
            (bool Success, int StatusCode, ErrorMessageDTO Message) todoItem = await _todoService.DeleteTodoItem(todoId);
            if (!todoItem.Success)
            {
                return StatusCode(todoItem.StatusCode, todoItem.Message);
            }

            return NoContent();
        }

        [HttpGet("todos")]
        public async Task<IActionResult> GetAllTodo(int page = 1, int limit = 10)
        {
            (bool Success, int StatusCode, ErrorMessageDTO Message, TodoListDTO todoList) todos = await _todoService.GetAllTodo(page, limit);
            if (!todos.Success)
            {
                return StatusCode(todos.StatusCode, todos.Message);
            }
            return Ok(todos.todoList);
        }
    }
}

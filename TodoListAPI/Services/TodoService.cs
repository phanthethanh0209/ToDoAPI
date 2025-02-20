using AutoMapper;
using TodoListAPI.DTOs;
using TodoListAPI.Models;
using TodoListAPI.Repositories;

namespace TodoListAPI.Services
{
    public interface ITodoService
    {
        Task<(bool Success, int StatusCode, ErrorMessageDTO Message, TodoItemResponseDTO TodoResponse)> CreateTodoItem(TodoDTO todoItem);
        Task<(bool Success, int StatusCode, ErrorMessageDTO Message, TodoItemResponseDTO TodoResponse)> UpdateTodoItem(int todoId, TodoDTO todoItem);
        Task<(bool Success, int StatusCode, ErrorMessageDTO Message)> DeleteTodoItem(int todoId);
        Task<(bool Success, int StatusCode, ErrorMessageDTO Message, TodoListDTO todoList)> GetAllTodo(int page, int limit);
        Task<TodoItemResponseDTO> GetTodoItem(int todoId);
    }
    public class TodoService : ITodoService
    {
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthService _authService;


        public TodoService(IRepositoryWrapper repository, IMapper mapper, IHttpContextAccessor contextAccessor, IAuthService authService)
        {
            _repository = repository;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
            _authService = authService;
        }

        public async Task<(bool Success, int StatusCode, ErrorMessageDTO Message, TodoItemResponseDTO TodoResponse)> CreateTodoItem(TodoDTO todoItem)
        {
            (bool Success, int StatusCode, ErrorMessageDTO Message, int? UserId) user = await _authService.Authentication();
            if (!user.Success)
            {
                return (false, user.StatusCode, user.Message, null);
            }

            Todo todo = _mapper.Map<Todo>(todoItem);
            todo.UserId = (int)user.UserId;

            await _repository.Todo.CreateAsync(todo);
            await _repository.SavechangeAsync();

            return (true, user.StatusCode, null, await GetTodoItem(todo.Id));
        }

        public async Task<TodoItemResponseDTO> GetTodoItem(int todoId)
        {
            Todo todo = _repository.Todo.SingleOrDefault(t => t.Id == todoId);

            if (todo == null)
            {
                return null;
            }
            return _mapper.Map<TodoItemResponseDTO>(todo);
        }

        public async Task<(bool Success, int StatusCode, ErrorMessageDTO Message, TodoItemResponseDTO TodoResponse)> UpdateTodoItem(int todoId, TodoDTO todoItem)
        {
            (bool Success, int StatusCode, ErrorMessageDTO Message, Todo todo) validationResult = await _authService.ValidateUserPermission(todoId);
            if (!validationResult.Success)
            {
                return (false, validationResult.StatusCode, validationResult.Message, null);
            }

            _mapper.Map(todoItem, validationResult.todo);

            await _repository.Todo.UpdateAsync(validationResult.todo);
            await _repository.SavechangeAsync();
            return (true, 200, null, await GetTodoItem(validationResult.todo.Id));
        }

        public async Task<(bool Success, int StatusCode, ErrorMessageDTO Message)> DeleteTodoItem(int todoId)
        {
            (bool Success, int StatusCode, ErrorMessageDTO Message, Todo todo) validationResult = await _authService.ValidateUserPermission(todoId);
            if (!validationResult.Success)
            {
                return (false, validationResult.StatusCode, validationResult.Message);
            }

            await _repository.Todo.DeleteAsync(validationResult.todo);
            await _repository.SavechangeAsync();
            return (true, 200, null);
        }

        public async Task<(bool Success, int StatusCode, ErrorMessageDTO Message, TodoListDTO todoList)> GetAllTodo(int page, int limit)
        {
            (bool Success, int StatusCode, ErrorMessageDTO Message, int? UserId) user = await _authService.Authentication();
            if (!user.Success)
            {
                return (false, user.StatusCode, user.Message, null);
            }

            IEnumerable<Todo> todos = await _repository.Todo.GetTodoListWithPaginationAsync(t => t.UserId == user.UserId, page, limit);
            List<TodoItemResponseDTO> todoDTOs = _mapper.Map<List<TodoItemResponseDTO>>(todos);
            int total = todoDTOs.Count;
            TodoListDTO todoListDTO = new()
            {
                data = todoDTOs,
                page = page,
                limit = limit,
                total = total
            };

            return (true, 200, null, todoListDTO);
        }
    }
}

using AutoMapper;
using TodoListAPI.DTOs;
using TodoListAPI.Models;
using TodoListAPI.Repositories;

namespace TodoListAPI.Services
{
    public interface ITodoService
    {
        Task<TodoItemResponseDTO?> CreateTodoItem(TodoDTO todoItem);
        Task<TodoItemResponseDTO?> UpdateTodoItem(int todoId, TodoDTO todoItem);
        Task<bool> DeleteTodoItem(int todoId);
        Task<TodoListDTO?> GetAllTodo(int page, int limit);
        Task<TodoItemResponseDTO?> GetTodoItem(int todoId);
        Task<TodoListDTO?> GetAllTodoByTitle(string title, int page, int limit);
        bool HasPermission(int userId, int todoId);
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

        public async Task<TodoItemResponseDTO?> CreateTodoItem(TodoDTO todoItem)
        {
            int? userId = _authService.GetUserForClaims();
            if (userId == null)
            {
                return null;
            }

            Todo todo = _mapper.Map<Todo>(todoItem);
            todo.UserId = userId.Value;

            await _repository.Todo.CreateAsync(todo);
            await _repository.SavechangeAsync();

            return await GetTodoItem(todo.Id);
        }

        public async Task<TodoItemResponseDTO?> GetTodoItem(int todoId)
        {
            Todo? todo = _repository.Todo.SingleOrDefault(t => t.Id == todoId);
            if (todo == null) return null;

            return _mapper.Map<TodoItemResponseDTO>(todo);
        }

        public async Task<TodoItemResponseDTO?> UpdateTodoItem(int todoId, TodoDTO todoItem)
        {
            Todo? todo = _repository.Todo.SingleOrDefault(t => t.Id == todoId);
            if (todo == null) return null;
            //DateTime? dueTime = todoItem.DueDate;

            //// giữ nguyên giá trị due date ban đầu khi cập nhật ko nhập gì
            //if (todoItem.DueDate == null && todo.DueDate != null)
            //{
            //    dueTime = todo.DueDate;
            //}

            _mapper.Map(todoItem, todo);
            todo.UpdatedDate = DateTime.Now;
            //todo.DueDate = dueTime;

            //await _repository.Todo.UpdateAsync(todo); EF Core đang tracking todo, thì bạn không cần gọi UpdateAsync().
            // EF tự động phát hiện thay đổi và update khi gọi SaveChangesAsync().
            await _repository.SavechangeAsync();
            return await GetTodoItem(todo.Id);
        }

        public async Task<bool> DeleteTodoItem(int todoId)
        {
            Todo? todo = _repository.Todo.SingleOrDefault(t => t.Id == todoId);
            if (todo == null) return false;

            await _repository.Todo.DeleteAsync(todo);
            await _repository.SavechangeAsync();
            return true;
        }

        public async Task<TodoListDTO?> GetAllTodo(int page, int limit)
        {
            int? userId = _authService.GetUserForClaims();
            if (userId == null)
            {
                return null;
            }

            IEnumerable<Todo> todos = await _repository.Todo.GetAllWithPaginationAsync(t => t.UserId == userId, page, limit);
            List<TodoItemResponseDTO> todoDTOs = _mapper.Map<List<TodoItemResponseDTO>>(todos);
            int total = todoDTOs.Count;
            TodoListDTO todoListDTO = new()
            {
                data = todoDTOs,
                page = page,
                limit = limit,
                total = total
            };

            return todoListDTO;
        }

        public bool HasPermission(int userId, int todoId)
        {
            Todo userIdAddTodo = _repository.Todo.SingleOrDefault(t => t.Id == todoId && t.UserId == userId);
            return userIdAddTodo != null ? true : false;
        }

        public async Task<TodoListDTO?> GetAllTodoByTitle(string title, int page, int limit)
        {
            int? userId = _authService.GetUserForClaims();
            if (userId == null)
            {
                return null;
            }

            IEnumerable<Todo> todos = await _repository.Todo.GetAllWithPaginationAsync(t => t.UserId == userId && t.Title.Contains(title), page, limit);
            if (todos == null)
                return null;

            List<TodoItemResponseDTO> todoDTOs = _mapper.Map<List<TodoItemResponseDTO>>(todos);
            TodoListDTO todoListDTO = new()
            {
                data = todoDTOs,
                page = page,
                limit = limit,
                total = todoDTOs.Count
            };

            return todoListDTO;
        }
    }
}

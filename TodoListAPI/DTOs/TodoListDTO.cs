namespace TodoListAPI.DTOs
{
    public class TodoListDTO
    {
        public List<TodoItemResponseDTO> data { get; set; } = new List<TodoItemResponseDTO>();
        public int page { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
    }
}

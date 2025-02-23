namespace TodoListAPI.DTOs
{
    public class TodoItemResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}

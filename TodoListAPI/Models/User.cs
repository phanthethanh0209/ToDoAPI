namespace TodoListAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // navigation property
        public ICollection<Todo> Todos { get; set; }
        //public ICollection<RefreshToken> refreshTokens { get; set; }
    }
}

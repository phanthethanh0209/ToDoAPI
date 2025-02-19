namespace TodoListAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }


        public int UserId { get; set; } // Foreign key
        public User User { get; set; } // navigation property
    }
}

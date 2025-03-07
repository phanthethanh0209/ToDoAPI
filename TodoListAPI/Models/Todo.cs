﻿namespace TodoListAPI.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        //public int Priority { get; set; }


        public int UserId { get; set; } // Foreign key
        public User User { get; set; }// navigation property
    }
}

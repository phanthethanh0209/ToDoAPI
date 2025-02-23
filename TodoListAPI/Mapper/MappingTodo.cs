using AutoMapper;
using TodoListAPI.DTOs;
using TodoListAPI.Models;

namespace TodoListAPI.Mapper
{
    public class MappingTodo : Profile
    {
        public MappingTodo()
        {
            CreateMap<TodoDTO, Todo>();
            CreateMap<Todo, TodoItemResponseDTO>();


        }
    }
}

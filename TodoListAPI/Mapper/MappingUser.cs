using AutoMapper;
using TodoListAPI.DTOs;
using TodoListAPI.Models;

namespace TodoListAPI.Mapper
{
    public class MappingUser : Profile
    {
        public MappingUser()
        {
            CreateMap<RegisterDTO, User>();
        }
    }
}

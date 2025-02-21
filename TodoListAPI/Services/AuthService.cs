﻿using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoListAPI.DTOs;
using TodoListAPI.Models;
using TodoListAPI.Repositories;

namespace TodoListAPI.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string ErrorMessage, TokenDTO token)> Register(RegisterDTO userDTO);
        Task<(bool Success, string ErrorMessage, TokenDTO token)> Login(LoginDTO userDTO);
        Task<(bool Success, int StatusCode, ErrorMessageDTO Message, int? UserId)> Authentication();
        Task<(bool Success, int StatusCode, ErrorMessageDTO Message, Todo todo)> ValidateUserPermission(int todoId);
        TokenDTO GenerateToken(User user);

    }
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly MyDBContext _dbContext;
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthService(IConfiguration config, MyDBContext dbContext, IRepositoryWrapper repository, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            _config = config;
            _dbContext = dbContext;
            _repository = repository;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
        }

        public async Task<(bool Success, string ErrorMessage, TokenDTO token)> Register(RegisterDTO userDTO)
        {
            bool isEmailUnique = await _repository.User.AnyAsync(m => m.Email == userDTO.Email);
            if (isEmailUnique)
            {
                return (false, "Email already exists ! ", null);
            }

            User user = _mapper.Map<User>(userDTO);
            user.Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password); // hash password

            await _repository.User.CreateAsync(user);
            await _repository.SavechangeAsync();

            TokenDTO token = GenerateToken(user);
            return (true, null, token);
        }


        public async Task<(bool Success, string ErrorMessage, TokenDTO token)> Login(LoginDTO userDTO)
        {
            User user = _repository.User.SingleOrDefault(u => u.Email == userDTO.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userDTO.Password, user.Password))
            {
                return (false, "Invalid email or password", null);
            }
            TokenDTO token = GenerateToken(user);
            return (true, null, token);
        }

        public TokenDTO GenerateToken(User user)
        {
            // Lấy cài đặt JWT từ cấu hình
            //IConfigurationSection jwtSettings = _configuration.GetSection("Jwt");
            //byte[] key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
            byte[] key = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]);

            JwtSecurityTokenHandler tokenHandler = new(); // tạo và xác thực JWT.
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                //Expires = DateTime.UtcNow.AddSeconds(20),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                                            SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor); // Tạo token
            string accessToken = tokenHandler.WriteToken(token); // Chuyển đổi token thành chuỗi


            return new TokenDTO
            {
                Token = accessToken
                //RefreshToken = refreshToken,
            };
        }

        public async Task<(bool Success, int StatusCode, ErrorMessageDTO Message, int? UserId)> Authentication()
        {
            HttpContext? httpContext = _contextAccessor.HttpContext;
            ErrorMessageDTO errorMessage = new();
            if (httpContext?.User?.Identity != null && httpContext.User.Identity.IsAuthenticated)
            {
                Claim? userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return (true, 200, null, userId);
                }
                else
                {
                    return (false, 500, new ErrorMessageDTO { Message = "Cannot retrieve the user ID from claims." }, null);
                }
            }
            else
            {
                return (false, 401, new ErrorMessageDTO { Message = "Unauthorized" }, null);
            }
        }

        public async Task<(bool Success, int StatusCode, ErrorMessageDTO Message, Todo todo)> ValidateUserPermission(int todoId)
        {
            (bool Success, int StatusCode, ErrorMessageDTO Message, int? UserId) user = await Authentication();
            if (!user.Success)
            {
                return (false, 401, user.Message, null);
            }

            Todo todo = _repository.Todo.SingleOrDefault(t => t.Id == todoId);

            if (todo == null)
            {
                return (false, 404, new ErrorMessageDTO { Message = "Todo not found" }, null);
            }

            if (!todo.UserId.Equals(user.UserId))
            {
                return (false, 403, new ErrorMessageDTO { Message = "Forbidden" }, null);
            }

            return (true, 200, null, todo);

        }
    }
}

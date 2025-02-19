using AutoMapper;
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
        TokenDTO GenerateToken(User user);
        Task<(bool Success, string ErrorMessage, TokenDTO token)> Login(LoginDTO userDTO);

    }
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly MyDBContext _dbContext;
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;

        public AuthService(IConfiguration config, MyDBContext dbContext, IRepositoryWrapper repository, IMapper mapper)
        {
            _config = config;
            _dbContext = dbContext;
            _repository = repository;
            _mapper = mapper;
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
                return (false, "Login failed", null);
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
                                                                 //string refreshToken = GenerateRefreshToken();

            //Lưu database
            //RefreshToken refreshTokenEntity = new()
            //{
            //    JwtId = token.Id,
            //    UserId = user.UserID,
            //    Token = refreshToken,
            //    IsUsed = false,
            //    IsRevoked = false,
            //    IsssueAt = DateTime.UtcNow,
            //    ExpiredAt = DateTime.UtcNow.AddHours(1),
            //};

            //await _repository.RefreshToken.CreateAsync(refreshTokenEntity);
            //await _repository.SaveChangeAsync();

            return new TokenDTO
            {
                Token = accessToken
                //RefreshToken = refreshToken,
            };
        }
    }
}

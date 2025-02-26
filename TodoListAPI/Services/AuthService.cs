using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TodoListAPI.DTOs;
using TodoListAPI.Models;
using TodoListAPI.Repositories;

namespace TodoListAPI.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? ErrorMessage, TokenDTO? token)> Register(RegisterDTO userDTO);
        Task<(bool Success, string? ErrorMessage, TokenDTO? token)> Login(LoginDTO userDTO);
        //Task<(bool Success, int StatusCode, ErrorMessageDTO Message, int? UserId)> Authentication();
        //Task<(bool Success, int StatusCode, ErrorMessageDTO Message, Todo todo)> ValidateUserPermission(int todoId);
        int? GetUserForClaims();

        TokenDTO GenerateToken(User user);
        Task<TokenDTO?> RefreshToken(string refreshToken);
        string GenerateRefreshToken();
        string GenerateAndSaveRefreshTokenAsync(User user);

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

        public async Task<(bool Success, string? ErrorMessage, TokenDTO? token)> Register(RegisterDTO userDTO)
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


        public async Task<(bool Success, string? ErrorMessage, TokenDTO? token)> Login(LoginDTO userDTO)
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
                AccessToken = accessToken,
                RefreshToken = GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        public async Task<TokenDTO?> RefreshToken(string refreshToken)
        {
            RefreshToken refresh = _repository.RefreshToken.SingleOrDefault(r => r.Token == refreshToken);

            if (refresh is null || refresh.IsRevoked || refresh.Expires < DateTime.Now)
            {
                return null;
            }

            User? user = await _repository.User.GetByIdAsync(t => t.Id == refresh.UserId);
            if (user == null)
            {
                return null;
            }

            refresh.IsRevoked = true;
            await _repository.RefreshToken.UpdateAsync(refresh);

            return GenerateToken(user);
        }

        public int? GetUserForClaims()
        {
            HttpContext? httpContext = _contextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            Claim? userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return null;
            }

            return userId;
        }

        public string GenerateRefreshToken()
        {
            byte[] randomNumber = new Byte[32];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public string GenerateAndSaveRefreshTokenAsync(User user)
        {
            string token = GenerateRefreshToken();
            RefreshToken refreshToken = new()
            {
                Token = token,
                IsRevoked = false,
                UserId = user.Id,
                Expires = DateTime.Now.AddDays(1)
            };
            _repository.RefreshToken.CreateAsync(refreshToken);
            _repository.SavechangeAsync();

            return token;
        }

    }
}

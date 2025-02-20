using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoListAPI.Mapper;
using TodoListAPI.Models;
using TodoListAPI.Repositories;
using TodoListAPI.Services;

namespace TodoListAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // Read config JWT from appsetting.json
            IConfigurationSection jwtSettings = builder.Configuration.GetSection("JwtSettings");
            byte[] secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            // config Authentication with JWT
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    //options.Events = new JwtBearerEvents
                    //{
                    //    OnChallenge = context =>
                    //    {
                    //        context.HandleResponse();
                    //        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    //        context.Response.ContentType = "application/json";
                    //        return context.Response.WriteAsync("{\"message\": \"Unauthorized\"}");
                    //    }
                    //};

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
                    };
                });


            // generate database (code first)
            IConfigurationRoot cf = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            builder.Services.AddDbContext<MyDBContext>(opt => opt.UseSqlServer(cf.GetConnectionString("MyDB")));

            // Register Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITodoService, TodoService>();

            // Register repository
            builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

            // Register autormapper
            builder.Services.AddAutoMapper(typeof(MappingUser));
            builder.Services.AddAutoMapper(typeof(MappingTodo));

            builder.Services.AddHttpContextAccessor();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication(); // add middleware authentication before authorization
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

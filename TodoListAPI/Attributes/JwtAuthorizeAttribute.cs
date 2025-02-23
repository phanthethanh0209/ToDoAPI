using Microsoft.AspNetCore.Mvc;
using TodoListAPI.Filters;

namespace TodoListAPI.Attributes
{
    public class JwtAuthorizeAttribute : TypeFilterAttribute
    {
        public JwtAuthorizeAttribute() : base(typeof(JwtAuthorizeFilter))
        {
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using TodoListAPI.Services;

namespace TodoListAPI.Filters
{
    public class JwtAuthorizeFilter : IAuthorizationFilter
    {
        private readonly ITodoService _todoService;

        public JwtAuthorizeFilter(ITodoService todoService)
        {
            _todoService = todoService;
        }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            // check authentication
            System.Security.Claims.ClaimsPrincipal user = context.HttpContext.User;
            Claim? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Unauthorized" });
                return;
            }

            // For the update, delete -> check forbidden 
            if (context.RouteData.Values.ContainsKey("todoId"))
            {
                if (context.RouteData.Values["todoId"] is string todoIdStr
                    && int.TryParse(todoIdStr, out int todoId))
                {
                    DTOs.TodoItemResponseDTO? t = await _todoService.GetTodoItem(todoId);
                    if (t == null)
                    {
                        context.Result = new NotFoundObjectResult(new { message = "Not found" });
                        return;
                    }

                    bool hasPermission = _todoService.HasPermission(userId, todoId);
                    if (!hasPermission)
                    {
                        context.Result = new ObjectResult(new { message = "Forbiddenn" })
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                        return;
                    }
                }
            }
        }
    }
}

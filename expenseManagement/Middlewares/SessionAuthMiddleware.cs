namespace expenseManagement.Middlewares
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Lấy path hiện tại
            var path = context.Request.Path.ToString().ToLower();

            // Bỏ qua các route không cần kiểm tra đăng nhập
            if (path.StartsWith("/account") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/images") ||
                path.StartsWith("/lib"))
            {
                await _next(context);
                return;
            }

            // Kiểm tra session
            var userId = context.Session.GetInt32("UserId");

            if (userId == null)
            {
                // Chưa đăng nhập, chuyển hướng về trang Login
                context.Response.Redirect("/Account/Login");
                return;
            }

            // Nếu đã đăng nhập thì cho phesp đi tiếp
            await _next(context);
        }
    }

    // Extension method để dễ dàng đăng ký middleware
    public static class SessionAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}

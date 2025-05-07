using Microsoft.AspNetCore.Builder;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCookieMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CookieMiddleware>();
    }
}
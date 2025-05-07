public class CookieMiddleware(RequestDelegate next, ILogger<CookieMiddleware> logger)
{
    private readonly ILogger<CookieMiddleware> _logger = logger;
    private readonly RequestDelegate _next = next;
    private const string CookieName = "ClientId";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(CookieName, out var clientId))
        {
            _logger.LogInformation("ClientId cookie not found. Generating a new one.");
            clientId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append(CookieName, clientId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
        else
        {
            _logger.LogInformation("ClientId cookie found: {ClientId}", clientId);
        }

        await _next(context);
    }
}
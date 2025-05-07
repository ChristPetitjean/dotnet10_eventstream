using System.Security.Claims;

public class UserNameIdentifierClaimsMiddleware(RequestDelegate next, ILogger<UserNameIdentifierClaimsMiddleware> logger)
{
    private readonly ILogger<UserNameIdentifierClaimsMiddleware> _logger = logger;
    private readonly RequestDelegate _next = next;
    private const string CookieName = "ClientId";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(CookieName, out var clientId))
        {
            _logger.LogInformation("ClientId cookie not found. Generating a new one.");
            clientId = Guid.NewGuid().ToString();
            context.User.AddIdentity(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, clientId)
            ]));
        }
        else
        {
            _logger.LogInformation("ClientId cookie found: {ClientId}", clientId);
        }

        await _next(context);
    }
}
namespace Netenberg.Api.Auth;

public sealed class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public ApiKeyAuthMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = context.Request.Headers[AuthConstants.ApiKeyHeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API key required");
            return;
        }

        if (apiKey != _configuration["PRIVATE_API_KEY"])
        {
            _logger.LogWarning("Invalid API Key attempt: {Key}", apiKey);
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Invalid API key");
            return;
        }

        await _next(context);
    }
}

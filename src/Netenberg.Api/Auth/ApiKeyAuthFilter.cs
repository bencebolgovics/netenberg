namespace Netenberg.Api.Auth;

public class ApiKeyAuthFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var extractedApiKey))
        {
            return Results.Unauthorized();
        }

        var apiKey = "asd";

        if (apiKey != extractedApiKey)
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}

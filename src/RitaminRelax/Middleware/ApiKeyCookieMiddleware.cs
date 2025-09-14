namespace RitaminRelax.Middleware;

public class ApiKeyCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyCookieMiddleware> _logger;

    public ApiKeyCookieMiddleware(RequestDelegate next, ILogger<ApiKeyCookieMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // 1. Cookie kiolvasása
            if (context.Request.Cookies.TryGetValue("RRApiKey", out var apiKey)
                && !string.IsNullOrWhiteSpace(apiKey))
            {
                // 2. SenseNet által elvárt header formátumba áthelyezés
                context.Request.Headers["X-Authentication-Type"] = "Apikey";
                context.Request.Headers["apikey"] = apiKey;

                // Log csak debug módban (ne loggolja az API key-t production-ben)
                _logger.LogDebug("API key átmozgatva cookie-ból header-be a kérés számára: {Path}",
                    context.Request.Path);
            }
            else
            {
                // Ha nincs API key cookie, az felhasználó nincs bejelentkezve
                _logger.LogDebug("Nincs RRApiKey cookie a kérésben: {Path}", context.Request.Path);
            }
        }
        catch (System.Exception ex)
        {
            // Ne bukjon el a middleware, csak loggolja a hibát
            _logger.LogError(ex, "Hiba történt az API key cookie feldolgozása során");
        }

        // 3. Következő middleware meghívása
        await _next(context);
    }
}

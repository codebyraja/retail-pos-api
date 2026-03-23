using Microsoft.Extensions.Caching.Memory;

namespace QsrAdminWebApi.Licensing;

public class LicenseMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, LicenseService licenseService, IMemoryCache cache)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        LicenseDto license;

        try
        {
            if (!cache.TryGetValue("LICENSE_STATUS", out license))
            {
                license = await licenseService.ValidateAsync();
                cache.Set("LICENSE_STATUS", license, TimeSpan.FromMinutes(5));
            }
        }
        catch
        {
            // 🔴 FAIL-CLOSED (NO LICENSE = BLOCK)
            context.Response.StatusCode = StatusCodes.Status423Locked;
            await context.Response.WriteAsJsonAsync(new
            {
                code = "LICENSE_CHECK_FAILED",
                message = "License validation failed. Please contact vendor."
            });
            return;
        }

        if (license == null || !license.Allowed)
        {
            context.Response.StatusCode = StatusCodes.Status423Locked;
            await context.Response.WriteAsJsonAsync(new
            {
                code = "LICENSE_LOCKED",
                message = license?.LockMessage ?? "Application access restricted"
            });
            return;
        }
        await _next(context);
    }
}

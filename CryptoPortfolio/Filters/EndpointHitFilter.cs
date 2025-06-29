using CryptoPorfolio.Filters.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CryptoPorfolio.Filters
{
    public sealed class EndpointHitFilter : IAsyncActionFilter
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _interval;

        public EndpointHitFilter(IMemoryCache cache, IOptions<EndpoinHitOptions> opts)
        {
            _cache = cache;
            _interval = TimeSpan.FromSeconds(opts.Value.Seconds);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.HttpContext.Request.Path.ToString().ToLowerInvariant();
            var key = $"{path}|{ip}";

            if (_cache.TryGetValue(key, out _))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
                return;
            }

            var executedCtx = await next();
            if (executedCtx.Result is IStatusCodeActionResult res)
            {
                if (res.StatusCode is 200)
                {
                    _cache.Set(key, true, _interval);
                }
            }
        }
    }
}
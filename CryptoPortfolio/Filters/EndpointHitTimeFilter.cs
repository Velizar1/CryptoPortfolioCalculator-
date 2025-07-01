using CryptoPorfolio.Filters.Options;
using CryptoPortfolio.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CryptoPorfolio.Filters
{
    public sealed class EndpointHitTimeFilter : IAsyncActionFilter
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _interval;

        public EndpointHitTimeFilter(IMemoryCache cache, IOptions<EndpoinHitOptions> opts)
        {
            _cache = cache;
            _interval = TimeSpan.FromSeconds(opts.Value.Seconds);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sessionId = context.HttpContext.Session.Id;
            var path = context.HttpContext.Request.Path.ToString().ToLowerInvariant();
            var key = $"{path}|{sessionId}";

            if (_cache.TryGetValue(key, out _))
            {

                context.Result = new ObjectResult(new { Error = $"{ErrorCodes.RequestsNotAllowedInTimeInterval}" })
                {
                    StatusCode = StatusCodes.Status429TooManyRequests
                };
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

using CryptoPorfolio.Filters;
using CryptoPorfolio.Filters.Options;
using CryptoPorfolio.Services;
using CryptoPorfolio.Services.Contracts;
using CryptoPortfolio.Infrastructure.Models;
using CryptoPortfolio.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Reflection;

namespace CryptoPorfolio
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMemoryCache();
            builder.Services.AddDistributedMemoryCache();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information().MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "Logs/operations-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    flushToDiskInterval: TimeSpan.FromSeconds(2))
                .CreateLogger();

            builder.Host.UseSerilog();
            // Add services to the container.
            builder.Services
                .Configure<CoinLoreOptionsModel>(builder.Configuration.GetSection("ExternalApis:CoinLore"));

            builder.Services
               .Configure<EndpoinHitOptions>(builder.Configuration.GetSection("EndpoinHit"));

            builder.Services.AddHttpClient<CoinLoreClient>()
                .ConfigureHttpClient((sp, http) =>
                {
                    var opt = sp.GetRequiredService<IOptions<CoinLoreOptionsModel>>().Value;
                    http.BaseAddress = new Uri(opt.BaseUrl);
                });

            builder.Services.AddScoped<IPortfolioManagerService, PortfolioManagerService>();
            builder.Services.AddScoped<EndpointHitTimeFilter>();
            builder.Services.AddHostedService<CoinLoreCacheSeeder>();
            builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
                                                     p.WithOrigins("https://localhost:5001", "http://localhost:3000")
                                                     .AllowAnyHeader()
                                                     .AllowAnyMethod()
                                                     .AllowCredentials()));
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);

                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(opts =>
            {
                opts.CustomOperationIds(api =>
                   api.ActionDescriptor.AttributeRouteInfo?.Name
                   ?? api.ActionDescriptor.RouteValues["action"]);

                opts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Crypto Portfolio",
                    Version = "v1"
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crypto API v1");
                    c.RoutePrefix = "swagger";
                });
                app.MapOpenApi();
            }

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseCors();
            app.UseSession();

            app.MapControllers();
            app.Run();
        }
    }
}

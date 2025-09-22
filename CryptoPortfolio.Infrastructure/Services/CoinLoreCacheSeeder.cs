using CryptoPortfolio.Services;
using CryptoPortfolio.Common.Constants;
using CryptoPortfolio.Common.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CryptoPortfolio.Common.Models.Cache;

namespace CryptoPortfolio.Infrastructure.Services
{
    public sealed class CoinLoreCacheSeeder : IHostedService
    {
        private short coinsPerBatch = 100;
        private readonly ICoinLoreClient _client;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CoinLoreCacheSeeder> _log;

        public CoinLoreCacheSeeder(
            ICoinLoreClient client,
            IMemoryCache cache,
            ILogger<CoinLoreCacheSeeder> log)
        {
            _client = client;
            _cache = cache;
            _log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ObservableHelper.TrackOperation(_log, $"{nameof(CoinLoreCacheSeeder)}-{nameof(StartAsync)}", () => SeedCache(cancellationToken));
        }

        public Task StopAsync(CancellationToken _)
        {
            return Task.CompletedTask;
        }

        private async Task SeedCache(CancellationToken cancellationToken)
        {
            var totalCoinsCount = await _client.GetGlobalCriptoCoinsCount(cancellationToken);
            if (totalCoinsCount == 0)
                return;

            List<Task<Dictionary<string, CacheCoinAPIModel>?>> tasks = new();
            var totalPages = totalCoinsCount / coinsPerBatch;

            for (var i = 0; i < totalPages; i++)
            {
                tasks.Add(_client.GetTickersPrices(i * coinsPerBatch, coinsPerBatch, cancellationToken));
            }
            
            var lastPageCount = totalCoinsCount % coinsPerBatch;
            if (lastPageCount > 0)
                tasks.Add(_client.GetTickersPrices(totalCoinsCount - lastPageCount, lastPageCount, cancellationToken));

            var results = (await Task.WhenAll(tasks))
                                     .Where(d => d != null)
                                     .SelectMany(d => d!);

            var coins = new Dictionary<string, CacheCoinAPIModel?>();
            foreach (var coin in results)
            {
                if (coins.ContainsKey(coin.Key))
                    continue;

                coins[coin.Key] = coin.Value;
            }

            _cache.Set(CacheConstants.CoinsKey, coins);
        }
    }
}

using CryptoPorfolio.Services.Contracts;
using CryptoPortfolio.Common.Constants;
using CryptoPortfolio.Common.Enums;
using CryptoPortfolio.Common.Helpers;
using CryptoPortfolio.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace CryptoPorfolio.Services
{
    public sealed class PortfolioManagerService : IPortfolioManagerService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<PortfolioManagerService> _logger;
        private readonly CoinLoreClient _client;

        public PortfolioManagerService(IMemoryCache cache,
            ILogger<PortfolioManagerService> logger,
            CoinLoreClient client)
        {
            _memoryCache = cache;
            _logger = logger;
            _client = client;
        }

        public async Task<ResultModel<List<PortfolioCoinModel>?>> ComputeCurrentPortfolio(IFormFile file, CancellationToken cancellationToken)
        {
            return await ObservableHelper.TrackOperation(_logger, $"{nameof(PortfolioManagerService)}-{nameof(ComputeCurrentPortfolio)}", () => ComputeResult(file, cancellationToken));
        }

        public async Task<ResultModel<List<PortfolioCoinModel>?>> UpdatePortfolio(CancellationToken cancellationToken)
        {
            return await ObservableHelper.TrackOperation(_logger, $"{nameof(PortfolioManagerService)}-{nameof(UpdatePortfolio)}", () => SyncPortfolioData(cancellationToken));
        }

        private async Task<ResultModel<List<PortfolioCoinModel>?>> SyncPortfolioData(CancellationToken cancellationToken)
        {
            if (_memoryCache.TryGetValue<Dictionary<string, CacheFileLineModel?>>(CacheConstants.FileCoinsKey, out var cachedFile) == false ||
                cachedFile == null)
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.MissingFileData);

            var ids = cachedFile.Keys.Select(x =>
            {
                int.TryParse(x, out int id);
                return id;
            });

            if (ids.Any(x => x == 0))
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.MissingFileData);

            var updatedResult = await _client.GetTickersPricesByIds(ids.ToList(), cancellationToken);

            if (updatedResult == null)
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.NoCoinDataFound);

            List<PortfolioCoinModel> result = new();

            foreach (var key in updatedResult.Keys)
            {
                var coinId = updatedResult[key].Id;
                var cachedFileCoin = cachedFile.GetValueOrDefault(coinId);
                if (cachedFileCoin == null)
                    return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.NoCoinDataFound);

                var updatedPrice = updatedResult[key].Price;

                result.Add(new PortfolioCoinModel
                {
                    CoinCount = cachedFileCoin!.Value.CoinCount,
                    BoughtValue = cachedFileCoin.Value.Price,
                    CoinCode = key,
                    CurrentValue = cachedFileCoin!.Value.CoinCount * updatedPrice,
                    PercentageChange = (updatedPrice - cachedFileCoin.Value.Price) / cachedFileCoin.Value.Price * 100,
                });
            }

            return ResultModel<List<PortfolioCoinModel>?>.Succeed(result);
        }

        private async Task<ResultModel<List<PortfolioCoinModel>?>> ComputeResult(IFormFile file, CancellationToken cancellationToken)
        {
            if (_memoryCache.TryGetValue<Dictionary<string, CacheCoinModel?>>(CacheConstants.CoinsKey, out var cache) == false)
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.MissingMemoryCache);

            _memoryCache.TryGetValue<Dictionary<string, CacheCoinModel?>?>(CacheConstants.DuplicatedCoinsKey, out var duplicatedCache);
            using var reader = new StreamReader(file.OpenReadStream());
            if (reader == null)
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.CouldNotCreateReader);

            string? line;
            List<PortfolioCoinModel> result = new();
            Dictionary<string, CacheFileLineModel?> fileInfo = new();

            while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
            {
                if (Regex.IsMatch(line, FileConstants.FileContextRegex) == false)
                    return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.LineNotInTheCorrectFormat);

                var values = line.Split("|");

                decimal coinPortCount;
                decimal coinPortValue;
                if (decimal.TryParse(values[0], out coinPortCount) == false ||
                    decimal.TryParse(values[2], out coinPortValue) == false)
                    return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.LineNotInTheCorrectFormat);

                var coinCode = values[1];

                var cachedCoin = cache!.GetValueOrDefault(coinCode, null);

                if (cachedCoin == null &&
                   (cachedCoin = duplicatedCache?.GetValueOrDefault(coinCode, null)) == null)
                    return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.MissingCoinInMemoryCache);

                result.Add(new PortfolioCoinModel
                {
                    CoinCount = coinPortCount,
                    BoughtValue = coinPortValue,
                    CoinCode = coinCode,
                    CurrentValue = coinPortCount * cachedCoin.Value.Price,
                    PercentageChange = (cachedCoin.Value.Price - coinPortValue) / coinPortValue * 100
                });

                fileInfo[cachedCoin.Value.Id] = new CacheFileLineModel
                {
                    Price = coinPortValue,
                    CoinCount = coinPortCount,
                };
            }

            _memoryCache.Set(CacheConstants.FileCoinsKey, fileInfo);
            return ResultModel<List<PortfolioCoinModel>?>.Succeed(result);
        }
    }
}
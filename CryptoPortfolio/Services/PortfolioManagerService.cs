using CryptoPorfolio.Models;
using CryptoPorfolio.Services.Contracts;
using CryptoPortfolio.Common.Constants;
using CryptoPortfolio.Common.Enums;
using CryptoPortfolio.Common.Helpers;
using CryptoPortfolio.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Drawing;
using System.Text.RegularExpressions;

namespace CryptoPorfolio.Services
{
    public sealed class PortfolioManagerService : IPortfolioManagerService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<PortfolioManagerService> _logger;

        public PortfolioManagerService(IMemoryCache cache, ILogger<PortfolioManagerService> logger)
        {
            _memoryCache = cache;
            _logger = logger;
        }

        public async Task<ResultModel<List<PortfolioCoinModel>?>> ComputeCurrentPortfolio(IFormFile file)
        {
            return await ObservableHelper.TrackOperation(_logger, $"{nameof(PortfolioManagerService)}-{nameof(ComputeCurrentPortfolio)}", () => ComputeResult(file));
        }

        private async Task<ResultModel<List<PortfolioCoinModel>?>> ComputeResult(IFormFile file)
        {
            if (_memoryCache.TryGetValue<Dictionary<string, CacheCoinModel?>>(CacheConstants.CoinsKey, out var cache) == false)
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.MissingMemoryCache);

            _memoryCache.TryGetValue<Dictionary<string, CacheCoinModel?>?>(CacheConstants.DuplicatedCoinsKey, out var duplicatedCache);
            using var reader = new StreamReader(file.OpenReadStream());
            if (reader == null)
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.CouldNotCreateReader);

            string? line;
            List<PortfolioCoinModel> result = new();
            Dictionary<string, CacheFileLineModel> fileInfo = new();

            while ((line = await reader.ReadLineAsync()) is not null)
            {
                if (Regex.IsMatch(line, FileConstants.FileContextRegex) == false)
                    return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.LineNotInTheCorrectFormat);

                var values = line.Split("|");

                decimal coinCount;
                decimal coinValue;
                if (decimal.TryParse(values[0], out coinCount) == false ||
                    decimal.TryParse(values[2], out coinValue) == false)
                    return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.LineNotInTheCorrectFormat);

                var coinCode = values[1];

                var cachedCoin = cache!.GetValueOrDefault(coinCode, null);

                if (cachedCoin == null &&
                   (cachedCoin = duplicatedCache?.GetValueOrDefault(coinCode, null)) == null)
                    return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.MissingCoinInMemoryCache);

                result.Add(new PortfolioCoinModel
                {
                    CoinCode = coinCode,
                    CurrentValue = coinCount * cachedCoin.Value.Price,
                    PercentageChange = (cachedCoin.Value.Price - coinValue) / coinValue * 100
                });

                fileInfo[cachedCoin.Value.Id] = new CacheFileLineModel
                {
                    Price = coinValue,
                    CoinCount = coinCount,
                };
            }

            _memoryCache.Set(CacheConstants.FileCoinsKey, fileInfo);
            return ResultModel<List<PortfolioCoinModel>?>.Succeed(result);
        }
    }
}
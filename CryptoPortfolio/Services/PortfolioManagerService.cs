using CryptoPortfolio.Services.Contracts;
using CryptoPortfolio.Common.Constants;
using CryptoPortfolio.Common.Enums;
using CryptoPortfolio.Common.Helpers;
using CryptoPortfolio.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace CryptoPortfolio.Services
{
    public sealed class PortfolioManagerService : IPortfolioManagerService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<PortfolioManagerService> _logger;
        private readonly ICoinLoreClient _client;

        public PortfolioManagerService(IMemoryCache cache,
            ILogger<PortfolioManagerService> logger,
            ICoinLoreClient client)
        {
            _memoryCache = cache;
            _logger = logger;
            _client = client;
        }

        public async Task<ResultModel<List<PortfolioCoinModel>?>> ComputeCurrentPortfolio(string chacheFileKey, IFormFile file, CancellationToken cancellationToken)
        {
            return await ObservableHelper.TrackOperation(
                _logger,
                $"{nameof(PortfolioManagerService)}-{nameof(ComputeCurrentPortfolio)}",
                () => ComputeResult(chacheFileKey, file, cancellationToken));
        }

        public async Task<ResultModel<List<PortfolioCoinModel>?>> UpdatePortfolio(string chacheFileKey, CancellationToken cancellationToken)
        {
            return await ObservableHelper.TrackOperation(
                _logger, 
                $"{nameof(PortfolioManagerService)}-{nameof(UpdatePortfolio)}",
                () => UpdatePortfolioData(chacheFileKey, cancellationToken));
        }

        private async Task<ResultModel<List<PortfolioCoinModel>?>> UpdatePortfolioData(string chacheFileKey, CancellationToken cancellationToken)
        {
            if (_memoryCache.TryGetValue<Dictionary<string, CacheFileLineModel?>>(chacheFileKey, out var cachedFile) == false ||
                cachedFile == null)
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.MissingFileData);

            var updatedCoinsResult = await _client.GetTickersPricesByIds(cachedFile.Keys.ToList(), cancellationToken);

            if (updatedCoinsResult == null)
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.NoCoinDataFound);

            List<PortfolioCoinModel> result = new();

            foreach (var key in updatedCoinsResult.Keys)
            {
                var coinId = updatedCoinsResult[key].Id;
                var cachedFileCoin = cachedFile.GetValueOrDefault(coinId);
                if (cachedFileCoin == null)
                    return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.NoCoinDataFound);

                var updatedPrice = updatedCoinsResult[key].Price;

                result.Add(new PortfolioCoinModel
                {
                    CoinCount = cachedFileCoin!.Value.CoinCount,
                    BoughtValue = cachedFileCoin.Value.Price,
                    CoinCode = key,
                    CurrentCoinValue = updatedPrice,
                    CurrentValue = cachedFileCoin!.Value.CoinCount * updatedPrice,
                    PercentageChange = (updatedPrice - cachedFileCoin.Value.Price) / cachedFileCoin.Value.Price * 100,
                });
            }

            return ResultModel<List<PortfolioCoinModel>?>.Succeed(result);
        }

        private async Task<ResultModel<List<PortfolioCoinModel>?>> ComputeResult(string chacheFileKey, IFormFile file, CancellationToken cancellationToken)
        {
            if (_memoryCache.TryGetValue<Dictionary<string, CacheCoinAPIModel?>>(CacheConstants.CoinsKey, out var cache) == false)
                return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.MissingMemoryCache);

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

                if (cachedCoin == null)
                    return ResultModel<List<PortfolioCoinModel>?>.Failed(ErrorCodes.MissingCoinInMemoryCache);

                result.Add(new PortfolioCoinModel
                {
                    CoinCount = coinPortCount,
                    BoughtValue = coinPortValue,
                    CurrentCoinValue = cachedCoin.Value.Price,
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

            _memoryCache.Set(chacheFileKey, fileInfo);
            return ResultModel<List<PortfolioCoinModel>?>.Succeed(result);
        }
    }
}
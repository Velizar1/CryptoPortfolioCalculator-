using CriptoPortfolio.Application.Coin;
using CriptoPortfolio.Application.Contracts;
using CryptoPortfolio.Application.Result;
using CryptoPortfolio.Application.Services.Contracts;
using CryptoPortfolio.Domain.Coin;
using CryptoPortfolio.Domain.Constants;
using CryptoPortfolio.Domain.Enums;
using CryptoPortfolio.Domain.Portfolio;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CryptoPortfolio.Application.Services
{
    public sealed class PortfolioManagerService : IPortfolioManagerService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<PortfolioManagerService> _logger;
        private readonly ICoinLoreClient _client;
        private readonly IObservableService _observableService;

        public PortfolioManagerService(IMemoryCache cache,
            ILogger<PortfolioManagerService> logger,
            IObservableService observableService,
            ICoinLoreClient client)
        {
            _observableService = observableService;
            _memoryCache = cache;
            _logger = logger;
            _client = client;
        }

        public async Task<ResultModel<List<PortfolioCryptoModel>?>> ComputeCurrentPortfolio(string chacheFileKey, IFormFile file, CancellationToken cancellationToken)
        {
            return await _observableService.TrackOperation(
                _logger,
                $"{nameof(PortfolioManagerService)}-{nameof(ComputeCurrentPortfolio)}",
                () => ComputeResult(chacheFileKey, file, cancellationToken));
        }

        public async Task<ResultModel<List<PortfolioCryptoModel>?>> UpdatePortfolio(string chacheFileKey, CancellationToken cancellationToken)
        {
            return await _observableService.TrackOperation(
                _logger,
                $"{nameof(PortfolioManagerService)}-{nameof(UpdatePortfolio)}",
                () => UpdatePortfolioData(chacheFileKey, cancellationToken));
        }

        private async Task<ResultModel<List<PortfolioCryptoModel>?>> UpdatePortfolioData(string chacheFileKey, CancellationToken cancellationToken)
        {
            if (_memoryCache.TryGetValue<List<CoinPortfolioModel>>(chacheFileKey, out var cachedFile) == false ||
                cachedFile == null)
                return ResultModel<List<PortfolioCryptoModel>?>.Failed(ErrorCodes.MissingFileData);

            var coinIds = cachedFile.Select(x => x.Coin.Id).ToList();

            var updatedCoinsResult = await _client.GetTickersPricesByIds(coinIds, cancellationToken);

            if (updatedCoinsResult == null)
                return ResultModel<List<PortfolioCryptoModel>?>.Failed(ErrorCodes.NoCoinDataFound);

            List<PortfolioCryptoModel> result = new();

            foreach (var key in updatedCoinsResult.Keys)
            {
                var coinId = updatedCoinsResult[key].Coin.Id;
                var cachedFileCoin = cachedFile.FirstOrDefault(x => x.Coin.Id == coinId);
                if (cachedFileCoin == null)
                    return ResultModel<List<PortfolioCryptoModel>?>.Failed(ErrorCodes.NoCoinDataFound);

                var updatedPrice = updatedCoinsResult[key].Price;
                var coinPrice = cachedFileCoin.Price;

                result.Add(new PortfolioCryptoModel
                {
                    CoinCount = cachedFileCoin.Count,
                    BoughtValue = coinPrice,
                    Coin = new CoinModel { Code = key, Id = coinId },
                    CurrentCoinValue = updatedPrice,
                    CurrentValue = cachedFileCoin.Count * updatedPrice,
                    PercentageChange = (updatedPrice - coinPrice) / coinPrice * 100,
                });
            }

            return ResultModel<List<PortfolioCryptoModel>?>.Succeed(result);
        }

        private async Task<ResultModel<List<PortfolioCryptoModel>?>> ComputeResult(string chacheFileKey, IFormFile file, CancellationToken cancellationToken)
        {
            if (_memoryCache.TryGetValue<List<CoinValueInfo>>(CacheConstants.CoinsKey, out var cache) == false)
                return ResultModel<List<PortfolioCryptoModel>?>.Failed(ErrorCodes.MissingMemoryCache);

            using var reader = new StreamReader(file.OpenReadStream());
            if (reader == null)
                return ResultModel<List<PortfolioCryptoModel>?>.Failed(ErrorCodes.CouldNotCreateReader);

            string? line;
            List<PortfolioCryptoModel> result = new();
            List<CoinPortfolioModel> fileInfo = new();

            while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
            {
                if (Regex.IsMatch(line, FileConstants.FileContextRegex) == false)
                    return ResultModel<List<PortfolioCryptoModel>?>.Failed(ErrorCodes.LineNotInTheCorrectFormat);

                var values = line.Split("|");

                if (decimal.TryParse(values[0], out decimal coinPortCount) == false ||
                    decimal.TryParse(values[2], out decimal coinPortValue) == false)
                    return ResultModel<List<PortfolioCryptoModel>?>.Failed(ErrorCodes.LineNotInTheCorrectFormat);

                var coinCode = values[1];

                var cachedCoin = cache!.FirstOrDefault(x => x.Coin.Code == coinCode);

                if (cachedCoin == null)
                    return ResultModel<List<PortfolioCryptoModel>?>.Failed(ErrorCodes.MissingCoinInMemoryCache);

                result.Add(new PortfolioCryptoModel
                {
                    CoinCount = coinPortCount,
                    BoughtValue = coinPortValue,
                    CurrentCoinValue = cachedCoin.Price,
                    Coin = new CoinModel { Code = coinCode, Id = cachedCoin.Coin.Id },
                    CurrentValue = coinPortCount * cachedCoin.Price,
                    PercentageChange = (cachedCoin.Price - coinPortValue) / coinPortValue * 100
                });

                var res = fileInfo.FirstOrDefault(x => x.Coin.Code == cachedCoin.Coin.Code);

                fileInfo.Add(new CoinPortfolioModel
                {
                    Price = coinPortValue,
                    Coin = new CoinModel { Code = coinCode, Id = cachedCoin.Coin.Id },
                    Count = coinPortCount
                });
            }

            _memoryCache.Set(chacheFileKey, fileInfo);
            return ResultModel<List<PortfolioCryptoModel>?>.Succeed(result);
        }
    }
}
using CryptoPorfolio.Services;
using CryptoPortfolio.Common.Constants;
using CryptoPortfolio.Common.Enums;
using CryptoPortfolio.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace CryptoPortfolio.Tests.Unit
{
    [TestFixture]
    public class PortfolioManagerServiceTests
    {
        private IMemoryCache _cache = null!;
        private PortfolioManagerService _svc = null!;
        private Mock<ICoinLoreClient> _clientMock = null!;
        private Mock<IMemoryCache> _memoryCacheMock = null!;
        private ILogger<PortfolioManagerService> _logger = null!;

        [OneTimeSetUp]
        public void SetUp()
        {
            _clientMock = new Mock<ICoinLoreClient>(MockBehavior.Strict);
            _memoryCacheMock = new Mock<IMemoryCache>(MockBehavior.Strict);
            _cache = new MemoryCache(new MemoryCacheOptions());
            _logger = new Mock<ILogger<PortfolioManagerService>>().Object;
            _svc = new PortfolioManagerService(_cache, _logger, _clientMock.Object);
        }

        [OneTimeTearDown]
        public void TearDow()
        {
            if(_cache != null)
                _cache.Dispose();
        }

        [Test]
        public async Task ComputeCurrentPortfolio_NoCache_ReturnsMissingMemoryCache()
        {
            var file = MakeFormFile("1|BTC|100");
            object? boxed = new Dictionary<string, CacheCoinAPIModel?>();

            _memoryCacheMock
                .Setup(m => m.TryGetValue(CacheConstants.CoinsKey, out boxed))
                .Returns(true);

            var service = new PortfolioManagerService(_memoryCacheMock.Object, _logger, _clientMock.Object);

            var result = await service.ComputeCurrentPortfolio("session1", file, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(ErrorCodes.MissingCoinInMemoryCache.ToString(), Is.EqualTo(result.ErrorMessage));
        }

        [Test]
        public async Task ComputeCurrentPortfolio_BadLineFormat_ReturnsLineNotInTheCorrectFormat()
        {
            _cache.Set(CacheConstants.CoinsKey,
                new Dictionary<string, CacheCoinAPIModel?> { { "BTC", new CacheCoinAPIModel { Id = "1", Price = 5m } } });

            var file = MakeFormFile("not|valid|line");
            var result = await _svc.ComputeCurrentPortfolio("session2", file, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(ErrorCodes.LineNotInTheCorrectFormat.ToString(), Is.EqualTo(result.ErrorMessage));
        }

        [Test]
        public async Task ComputeCurrentPortfolio_MissingCoinInCache_ReturnsMissingCoinInMemoryCache()
        {
            _cache.Set(CacheConstants.CoinsKey,
                new Dictionary<string, CacheCoinAPIModel?>());

            var file = MakeFormFile("2|ETH|200");
            var result = await _svc.ComputeCurrentPortfolio("session3", file, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(ErrorCodes.MissingCoinInMemoryCache.ToString(), Is.EqualTo(result.ErrorMessage));
        }

        [Test]
        public async Task ComputeCurrentPortfolio_ValidFile_SucceedsAndCachesFileInfo()
        {
            var coinBoughtValue = 50m;
            var coinCount = 3;
            var coinValue = 100m;
            var apiCache = new Dictionary<string, CacheCoinAPIModel?> {
                { "BTC", new CacheCoinAPIModel { Id = "42", Price = coinValue } }
            };
            _cache.Set(CacheConstants.CoinsKey, apiCache);

            var file = MakeFormFile("3|BTC|50");
            var result = await _svc.ComputeCurrentPortfolio("session4", file, CancellationToken.None);

            
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            
            var coin = result.Data![0];
            
            Assert.That("BTC", Is.EqualTo(coin.CoinCode));
            Assert.That(coinValue, Is.EqualTo(coin.CurrentCoinValue));
            Assert.That(coinCount * coinValue, Is.EqualTo(coin.CurrentValue));
            Assert.That((coinValue - coinBoughtValue) / coinBoughtValue * 100, Is.EqualTo(coin.PercentageChange));

            Assert.That(_cache.TryGetValue<Dictionary<string, CacheFileLineModel?>>(
                "session4", out var fileInfo), Is.True);
            Assert.That(fileInfo, Is.Not.Null);

            var entry = fileInfo!["42"]!;
            
            Assert.That(coinCount, Is.EqualTo(entry.Value.CoinCount));
            Assert.That(coinBoughtValue, Is.EqualTo(entry.Value.Price));
        }

        [Test]
        public async Task UpdatePortfolio_NoFileInCache_ReturnsMissingFileData()
        {
            var result = await _svc.UpdatePortfolio("noSession", CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(ErrorCodes.MissingFileData.ToString(), Is.EqualTo(result.ErrorMessage));
        }

        [Test]
        public async Task UpdatePortfolio_ClientReturnsNull_ReturnsNoCoinDataFound()
        {
            // arrange
            var fileInfo = new Dictionary<string, CacheFileLineModel?> {
                { "BTC", new CacheFileLineModel { Price = 10m, CoinCount = 2m } }
            };
            _cache.Set("sess1", fileInfo);

            _clientMock
                .Setup(c => c.GetTickersPricesByIds(
                    It.IsAny<List<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Dictionary<string, CacheCoinAPIModel>?)null);

            var result = await _svc.UpdatePortfolio("sess1", CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(ErrorCodes.NoCoinDataFound.ToString(), Is.EqualTo(result.ErrorMessage));
        }

        [Test]
        public async Task UpdatePortfolio_MissingCacheEntryForCoin_ReturnsNoCoinDataFound()
        {
            var fileInfo = new Dictionary<string, CacheFileLineModel?> {
                { "USD", new CacheFileLineModel { Price = 1m, CoinCount = 100m } }
            };
            _cache.Set("sess2", fileInfo);

            var prices = new Dictionary<string, CacheCoinAPIModel> {
                { "BTC", new CacheCoinAPIModel { Id = "99", Price = 5m } }
            };
            _clientMock
                .Setup(c => c.GetTickersPricesByIds(
                    It.IsAny<List<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(prices);

            var result = await _svc.UpdatePortfolio("sess2", CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(ErrorCodes.NoCoinDataFound.ToString(), Is.EqualTo(result.ErrorMessage));
        }

        [Test]
        public async Task UpdatePortfolio_ValidData_Succeeds()
        {
            var coinCount = 3m;
            var coinValue = 20m;
            var coinBoughtValue = 10m;
            var fileInfo = new Dictionary<string, CacheFileLineModel?> {
                { "BTC", new CacheFileLineModel { Price = coinBoughtValue, CoinCount = coinCount } }
            };
            _cache.Set("sess3", fileInfo);

            var prices = new Dictionary<string, CacheCoinAPIModel> {
                { "BTC", new CacheCoinAPIModel { Id = "BTC", Price = coinValue } }
            };

            _clientMock
                .Setup(c => c.GetTickersPricesByIds(
                    It.IsAny<List<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(prices);

            var result = await _svc.UpdatePortfolio("sess3", CancellationToken.None);


            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            var coin = result.Data![0];
            Assert.That("BTC", Is.EqualTo(coin.CoinCode));
            Assert.That(coinCount, Is.EqualTo(coin.CoinCount));
            Assert.That(coinValue, Is.EqualTo(coin.CurrentCoinValue));
            Assert.That(coinCount * coinValue, Is.EqualTo(coin.CurrentValue));
            Assert.That((coinValue - coinBoughtValue) / coinBoughtValue * 100, Is.EqualTo(coin.PercentageChange));
        }

        private static IFormFile MakeFormFile(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", "test.txt");
        }
    }
}

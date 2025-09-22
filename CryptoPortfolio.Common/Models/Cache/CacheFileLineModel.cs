namespace CryptoPortfolio.Common.Models.Cache
{
    public record struct CacheFileLineModel
    {
        public decimal Price { get; init; }

        public decimal CoinCount { get; init; }
    }
}

namespace CryptoPortfolio.Common.Models
{
    public record struct CacheFileLineModel
    {
        public decimal Price { get; init; }

        public decimal CoinCount { get; init; }
    }
}

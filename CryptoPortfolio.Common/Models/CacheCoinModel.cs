namespace CryptoPortfolio.Common.Models
{
    public record struct CacheCoinModel
    {
        public required string Id { get; init; }
        public decimal Price { get; init; }
    }
}

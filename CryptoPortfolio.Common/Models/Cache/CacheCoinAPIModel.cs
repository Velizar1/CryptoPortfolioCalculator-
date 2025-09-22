namespace CryptoPortfolio.Common.Models.Cache
{
    public record struct CacheCoinAPIModel
    {
        public required string Id { get; init; }

        public decimal Price { get; init; }
    }
}

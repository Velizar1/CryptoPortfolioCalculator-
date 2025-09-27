namespace CryptoPortfolio.Domain.Coin
{
    public record struct CoinModel
    {
        public int Id { get; init; }

        public required string Code { get; init; }
    }
}

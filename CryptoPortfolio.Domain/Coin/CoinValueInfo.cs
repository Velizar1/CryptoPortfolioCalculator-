namespace CryptoPortfolio.Domain.Coin
{
    public record CoinValueInfo
    {
        public required CoinModel Coin { get; init; }

        public required  decimal Price { get; init; }
    }
}

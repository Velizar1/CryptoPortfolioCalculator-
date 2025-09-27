using CryptoPortfolio.Domain.Coin;

namespace CryptoPortfolio.Domain.Portfolio
{
    public class PortfolioCryptoModel
    {
        public decimal CoinCount { get; init; }

        public decimal BoughtValue { get; init; }

        public decimal CurrentCoinValue { get; init; }

        public decimal CurrentValue { get; init; }

        public required CoinModel Coin { get; init; }

        public decimal PercentageChange { get; init; }
    }
}

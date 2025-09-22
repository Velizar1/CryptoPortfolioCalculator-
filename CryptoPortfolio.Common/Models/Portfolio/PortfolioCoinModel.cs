namespace CryptoPortfolio.Common.Models.Portfolio
{
    public record class PortfolioCoinModel
    {
        public decimal CoinCount { get; init; }

        public decimal BoughtValue { get; init; }

        public decimal CurrentCoinValue { get; init; }

        public decimal CurrentValue { get; init; }

        public required string CoinCode { get; init; }

        public decimal PercentageChange { get; init; }
    }
}

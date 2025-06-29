namespace CryptoPortfolio.Common.Models
{
    public record class PortfolioCoinModel
    {
        public decimal CurrentValue { get; init; }

        public required string CoinCode { get; init; }

        public decimal PercentageChange { get; init; }
    }
}

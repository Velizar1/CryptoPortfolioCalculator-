using CryptoPortfolio.Domain.Coin;

namespace CriptoPortfolio.Application.Coin
{
    public record CoinPortfolioModel : CoinValueInfo
    {
        public decimal Count { get; set; }
    }
}

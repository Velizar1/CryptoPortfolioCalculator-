
using CryptoPortfolio.Domain.Coin;

namespace CryptoPortfolio.Application
{
    public interface ICoinLoreClient
    {
        Task<int> GetGlobalCriptoCoinsCount(CancellationToken token);
        Task<Dictionary<string, CoinValueInfo>?> GetTickersPrices(int skip, int take, CancellationToken cancelationToken);
        Task<Dictionary<string, CoinValueInfo>?> GetTickersPricesByIds(List<int> ids, CancellationToken cancelationToken);
    }
}
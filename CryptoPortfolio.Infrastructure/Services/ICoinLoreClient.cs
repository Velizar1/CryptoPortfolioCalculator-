using CryptoPortfolio.Common.Models;

namespace CryptoPorfolio.Services
{
    public interface ICoinLoreClient
    {
        Task<int> GetGlobalCriptoCoinsCount(CancellationToken token);
        Task<Dictionary<string, CacheCoinAPIModel>?> GetTickersPrices(int skip, int take, CancellationToken cancelationToken);
        Task<Dictionary<string, CacheCoinAPIModel>?> GetTickersPricesByIds(List<string> ids, CancellationToken cancelationToken);
    }
}
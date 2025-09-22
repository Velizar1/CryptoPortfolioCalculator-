using CryptoPortfolio.Common.Models.Cache;

namespace CryptoPortfolio.Services
{
    public interface ICoinLoreClient
    {
        Task<int> GetGlobalCriptoCoinsCount(CancellationToken token);
        Task<Dictionary<string, CacheCoinAPIModel>?> GetTickersPrices(int skip, int take, CancellationToken cancelationToken);
        Task<Dictionary<string, CacheCoinAPIModel>?> GetTickersPricesByIds(List<string> ids, CancellationToken cancelationToken);
    }
}
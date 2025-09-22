using CryptoPortfolio.Common.Constants;
using CryptoPortfolio.Common.Models.Cache;
using CryptoPortfolio.Infrastructure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace CryptoPortfolio.Services
{
    public sealed class CoinLoreClient : ICoinLoreClient
    {
        private readonly HttpClient _client;

        public CoinLoreClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<int> GetGlobalCriptoCoinsCount(CancellationToken token)
        {
            using var response = await _client.GetAsync(ExternalCryptocurrencyAPIConstants.Global, token);
            _ = response.EnsureSuccessStatusCode();

            var resultStr = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<CoinLoreGlobalData>>(resultStr)!;

            var isEmptyResult = data == null ||
                data.Count() == 0;

            return isEmptyResult ? 0 : data!.First().CoinsCount;
        }

        public Task<Dictionary<string, CacheCoinAPIModel>?> GetTickersPrices(int skip, int take, CancellationToken cancelationToken)
        {
            var uri = String.Format(ExternalCryptocurrencyAPIConstants.TickersBySize, skip, take);
            return GetResponse(uri, cancelationToken);
        }

        public async Task<Dictionary<string, CacheCoinAPIModel>?> GetTickersPricesByIds(List<string> ids, CancellationToken cancelationToken)
        {
            var strIds = string.Join(",", ids);
            var uri = String.Format(ExternalCryptocurrencyAPIConstants.TickersByIds, strIds);
            return await GetResponse(uri, cancelationToken);
        }

        private async Task<Dictionary<string, CacheCoinAPIModel>?> GetResponse(string uri, CancellationToken cancelationToken)
        {
            using var response = await _client.GetAsync(uri, cancelationToken);
            _ = response.EnsureSuccessStatusCode();

            var resultStr = await response.Content.ReadAsStringAsync();

            var token = JToken.Parse(resultStr);

            List<CoinLoreTicker>? data = token switch
            {
                JArray arr => arr.ToObject<List<CoinLoreTicker>>(),
                JObject obj when obj["data"] is JArray tickersArr
                                              => tickersArr.ToObject<List<CoinLoreTicker>>(),
                _ => null
            };

            if (data == null || data.Count() == 0)
                return null;

            return data
                     .GroupBy(x => x.Symbol)
                     .ToDictionary(x => x.Key, x =>
                     {
                         var first = x.First();
                         decimal.TryParse(first.PriceUsd,
                                            NumberStyles.Float,
                                            CultureInfo.InvariantCulture,
                                            out var price);

                         return new CacheCoinAPIModel
                         {
                             Id = first.Id,
                             Price = price
                         };
                     });
        }
    }
}

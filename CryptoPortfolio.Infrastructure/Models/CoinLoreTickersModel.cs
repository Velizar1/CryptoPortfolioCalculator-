using Newtonsoft.Json;

namespace CryptoPortfolio.Infrastructure.Models
{
    public record CoinLoreTicker
    {
        [JsonProperty("id")]
        public string Id { get; init; } = default!;

        [JsonProperty("symbol")]
        public string Symbol { get; init; } = default!;

        [JsonProperty("name")]
        public string Name { get; init; } = default!;

        [JsonProperty("nameid")]
        public string NameId { get; init; } = default!;

        [JsonProperty("rank")]
        public int Rank { get; init; }

        [JsonProperty("price_usd")]
        public string PriceUsd { get; init; } = default!;

        [JsonProperty("percent_change_24h")]
        public string PercentChangeOneDay { get; init; } = default!;

        [JsonProperty("percent_change_1h")]
        public string PercentChangePerHour { get; init; } = default!;

        [JsonProperty("percent_change_7d")]
        public string PercentChangeOneWeek { get; init; } = default!;

        [JsonProperty("price_btc")]
        public string PriceBtc { get; init; } = default!;

        [JsonProperty("market_cap_usd")]
        public string MarketCapUsd { get; init; } = default!;

        [JsonProperty("volume24")]
        public decimal? VolumeInOneDay { get; init; } 

        [JsonProperty("volume24a")]
        public decimal? VolumeInOneDayA { get; init; }

        [JsonProperty("csupply")]
        public string Csupply { get; init; } = default!;

        [JsonProperty("tsupply")]
        public string Tsupply { get; init; } = default!;

        [JsonProperty("msupply")]
        public string Msupply { get; init; } = default!;
    }

}

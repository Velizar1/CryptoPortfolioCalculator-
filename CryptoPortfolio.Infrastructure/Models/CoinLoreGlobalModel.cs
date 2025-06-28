using Newtonsoft.Json;

namespace CryptoPortfolio.Infrastructure.Models
{
    public record CoinLoreGlobalData
    {
        [JsonProperty("coins_count")]
        public int CoinsCount { get; init; }

        [JsonProperty("active_markets")]
        public int ActiveMarkets { get; init; }

        [JsonProperty("total_mcap")]
        public decimal TotalMcap { get; init; }

        [JsonProperty("total_volume")]
        public decimal TotalVolume { get; init; }

        [JsonProperty("btc_d")]
        public string? BtcD { get; init; }

        [JsonProperty("eth_d")]
        public string? TthD { get; init; }

        [JsonProperty("mcap_change")]
        public string? McapChange { get; init; }

        [JsonProperty("volume_change")]
        public string? VolumeChange { get; init; }

        [JsonProperty("avg_change_percent")]
        public string? AvgChangePercent { get; init; }

        [JsonProperty("volume_ath")]
        public long VolumeAth { get; init; }

        [JsonProperty("mcap_ath")]
        public decimal McapAth { get; init; }
    }

}

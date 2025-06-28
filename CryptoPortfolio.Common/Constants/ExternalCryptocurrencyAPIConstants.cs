using System.Diagnostics;

namespace CryptoPortfolio.Common.Constants
{
    public static class ExternalCryptocurrencyAPIConstants
    {
        public const string Global = @"/api/global/";
        public const string TickersBySize = @"/api/tickers/?start={0}&limit={1}";
        public const string TickersByIds = @"/api/ticker/?id={0}";
    }
}

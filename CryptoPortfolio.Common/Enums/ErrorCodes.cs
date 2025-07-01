namespace CryptoPortfolio.Common.Enums
{
    public enum ErrorCodes
    {
        MissingMemoryCache = 1,
        CouldNotCreateReader = 2,
        LineNotInTheCorrectFormat = 3,
        MissingCoinInMemoryCache = 4,
        MissingFileData = 5,
        NoCoinDataFound = 6,
        RequestsNotAllowedInTimeInterval = 7
    }
}

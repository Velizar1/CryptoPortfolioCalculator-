namespace CryptoPortfolio.Common.Constants
{
    public static class FileConstants
    {
        public const string SupportedFormat = ".txt";
        public const string FileContextRegex = @"^([0-9]+(?:\.[0-9]+)?)\|[A-Z]{2,5}\|([0-9]+(?:\.[0-9]+)?)$";
        public const int MaxFileUploadSizeInBytes = 5 * 1024 * 1024;
    }
}

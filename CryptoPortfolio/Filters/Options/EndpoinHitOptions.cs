namespace CryptoPorfolio.Filters.Options
{
    public record EndpoinHitOptions
    {
        public int Seconds { get; init; } = 300; // 5 minutes
    }
}

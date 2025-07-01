using CryptoPortfolio.Common.Models;

namespace CryptoPorfolio.Services.Contracts
{
    public interface IPortfolioManagerService
    {
        Task<ResultModel<List<PortfolioCoinModel>?>> ComputeCurrentPortfolio(string chacheFileKey, IFormFile file, CancellationToken cancellationToken);

        Task<ResultModel<List<PortfolioCoinModel>?>> UpdatePortfolio(string chacheFileKey, CancellationToken cancellationToken);
    }
}

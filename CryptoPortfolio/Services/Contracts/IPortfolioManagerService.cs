using CryptoPortfolio.Common.Models;

namespace CryptoPorfolio.Services.Contracts
{
    public interface IPortfolioManagerService
    {
        Task<ResultModel<List<PortfolioCoinModel>?>> ComputeCurrentPortfolio(IFormFile file, CancellationToken cancellationToken);

        Task<ResultModel<List<PortfolioCoinModel>?>> UpdatePortfolio(CancellationToken cancellationToken);
    }
}

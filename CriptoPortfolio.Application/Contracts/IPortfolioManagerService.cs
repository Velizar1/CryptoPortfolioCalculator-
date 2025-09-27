using CryptoPortfolio.Application.Result;
using CryptoPortfolio.Domain.Portfolio;
using Microsoft.AspNetCore.Http;

namespace CryptoPortfolio.Application.Services.Contracts
{
    public interface IPortfolioManagerService
    {
        Task<ResultModel<List<PortfolioCryptoModel>?>> ComputeCurrentPortfolio(string chacheFileKey, IFormFile file, CancellationToken cancellationToken);

        Task<ResultModel<List<PortfolioCryptoModel>?>> UpdatePortfolio(string chacheFileKey, CancellationToken cancellationToken);
    }
}

using CryptoPorfolio.Models;
using CryptoPortfolio.Common.Models;
using System.Threading.Tasks;

namespace CryptoPorfolio.Services.Contracts
{
    public interface IPortfolioManagerService
    {
        Task<ResultModel<List<PortfolioCoinModel>?>> ComputeCurrentPortfolio(IFormFile file);
    }
}

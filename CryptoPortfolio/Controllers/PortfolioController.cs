using CryptoPorfolio.Filters;
using CryptoPorfolio.Services.Contracts;
using CryptoPortfolio.Common.Constants;
using CryptoPortfolio.Common.Models;
using Microsoft.AspNetCore.Mvc;
namespace CryptoPorfolio.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioManagerService _portfolioManager;
        public PortfolioController(IPortfolioManagerService portfolioManager)
        {
            _portfolioManager = portfolioManager;
        }

        [HttpPut]
        [ProducesResponseType(typeof(ResultModel<List<PortfolioCoinModel>?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<PortfolioCoinModel>?>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<PortfolioCoinModel>?>> UploadCryptoPortfolio(IFormFile file, CancellationToken cancellationToken)
        {
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("File too large.");

            if (!Path.GetExtension(file.FileName).Equals(FileConstants.SupportedFormat, StringComparison.OrdinalIgnoreCase))
                return BadRequest($"Only {FileConstants.SupportedFormat} allowed.");

            var result = await _portfolioManager.ComputeCurrentPortfolio(file, cancellationToken);
            return result;
        }

        [HttpGet]
        [ServiceFilter(typeof(EndpointHitFilter))] // 60-sec window
        [ProducesResponseType(typeof(ResultModel<List<PortfolioCoinModel>?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<PortfolioCoinModel>?>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<PortfolioCoinModel>?>> RefreshInformation(CancellationToken cancellationToken)
        {
            var result = await _portfolioManager.UpdatePortfolio(cancellationToken);
            return result;
        }
    }
}

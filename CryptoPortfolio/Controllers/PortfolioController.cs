using CryptoPortfolio.Filters;
using CryptoPortfolio.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using CryptoPortfolio.Common.Models.Portfolio;
using CryptoPortfolio.Common.Models.Result;
using CryptoPortfolio.Application.Services.Contracts;
using CryptoPortfolio.Domain.Portfolio;
namespace CryptoPortfolio.Controllers
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
        [ProducesResponseType(typeof(ResultModel<List<PortfolioCryptoModel>?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<PortfolioCryptoModel>?>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<PortfolioCryptoModel>?>> UploadCryptoPortfolio(IFormFile file, CancellationToken cancellationToken)
        {
            if (file.Length > FileConstants.MaxFileUploadSizeInBytes)
                return BadRequest("File too large.");

            if (!Path.GetExtension(file.FileName).Equals(FileConstants.SupportedFormat, StringComparison.OrdinalIgnoreCase))
                return BadRequest($"Only {FileConstants.SupportedFormat} allowed.");
            HttpContext.Session.SetString("Init", "1");
            string sessionId = HttpContext.Session.Id;
            var result = await _portfolioManager.ComputeCurrentPortfolio(sessionId, file, cancellationToken);

            return result;
        }

        [HttpGet]
        [ServiceFilter(typeof(EndpointHitTimeFilter))] // 55-sec hit interval allowed
        [ProducesResponseType(typeof(ResultModel<List<PortfolioCryptoModel>?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<PortfolioCryptoModel>?>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<PortfolioCryptoModel>?>> RefreshInformation(CancellationToken cancellationToken)
        {
            string sessionId = HttpContext.Session.Id;
            var result = await _portfolioManager.UpdatePortfolio(sessionId, cancellationToken);
            return result;
        }
    }
}

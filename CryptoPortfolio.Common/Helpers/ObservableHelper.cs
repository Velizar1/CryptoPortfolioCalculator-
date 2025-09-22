using CryptoPortfolio.Common.Models.Result;
using Microsoft.Extensions.Logging;

namespace CryptoPortfolio.Common.Helpers
{
    public static class ObservableHelper
    {
        public static async Task TrackOperation<T>(ILogger<T> logger, string name, Func<Task> func)
        {
            logger.LogInformation($"Start {name}");
            try
            {
                await func();

                logger.LogInformation($"Finish {name}\n");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{name}\n");
                throw;
            }
        }

        public static async Task<ResultModel<Y>> TrackOperation<T, Y>(ILogger<T> logger, string name, Func<Task<ResultModel<Y>>> func)
        {
            logger.LogInformation($"Start {name}");

            try
            {
                var result = await func();
                if (result.Success == false)
                    logger.LogError(result.ErrorMessage);

                logger.LogInformation($"Finish {name}\n");
                
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{name}\n");
                throw;
            }
        }
    }
}

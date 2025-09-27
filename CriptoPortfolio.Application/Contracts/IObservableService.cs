using CryptoPortfolio.Application.Result;
using Microsoft.Extensions.Logging;

namespace CriptoPortfolio.Application.Contracts
{
    public interface IObservableService
    {
        public Task TrackOperation<T>(ILogger<T> logger, string name, Func<Task> func);

        public Task<ResultModel<Y>> TrackOperation<T, Y>(ILogger<T> logger, string name, Func<Task<ResultModel<Y>>> func);
    }
}

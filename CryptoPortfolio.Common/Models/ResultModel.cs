using CryptoPortfolio.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CryptoPortfolio.Common.Models
{
    public class ResultModel
    {
        public string? ErrorMessage { get; set; }
        public bool Success { get; set; }

        public ResultModel(string? errorMessage, bool success)
        {
            ErrorMessage = errorMessage;
            Success = success;
        }
    }


    public class ResultModel<TData> : ResultModel
    {
        public TData? Data { get; set; }

        private ResultModel(bool succeeded, TData? data, string? errorMessage = null)
           : base(errorMessage, succeeded)
        {
            this.Data = data;
        }

        public static ResultModel<TData> Failed(ErrorCodes error)
          => new ResultModel<TData>(false, default, error.ToString());

        public static ResultModel<TData> Succeed(TData data)
           => new ResultModel<TData>(true, data, default);

        public static implicit operator ActionResult<TData?>(ResultModel<TData> result)
        {
            if (result.Success == false)
            {
                return new BadRequestObjectResult(result.ErrorMessage);
            }

            return result.Data;
        }
    }
}

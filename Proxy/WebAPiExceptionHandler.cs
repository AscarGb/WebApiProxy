using Proxy.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace Proxy
{
    public class WebAPiExceptionHandler: IExceptionHandler
    {
        private readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            var errorInfo = new ErrorInformation
            {
                Message = "Произошла ошибка, смотрите log.txt",
                ErrorDate = DateTime.UtcNow
            };

            Logger.ErrorException("Ошибка: ", context.Exception);

            var httpResponse = context.Request.CreateResponse(HttpStatusCode.InternalServerError, errorInfo);

            context.Result = new ResponseMessageResult(httpResponse);

            return Task.FromResult(0);
        }

        private class ErrorInformation
        {
            public string Message { get; set; }
            public DateTime ErrorDate { get; set; }
        }
    }
}
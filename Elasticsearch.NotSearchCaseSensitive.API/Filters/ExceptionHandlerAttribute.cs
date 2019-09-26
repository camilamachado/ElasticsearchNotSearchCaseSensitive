using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Elasticsearch.NotSearchCaseSensitive.API.Exceptions;

namespace Elasticsearch.NotSearchCaseSensitive.API.Filters
{
    public class ExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// Método invocado quando ocorre uma exceção no controller
        /// </summary>
        /// <param name="context">É o contexto atual da requisição</param>
        public override void OnException(ExceptionContext context)
        {
            context.Exception = context.Exception;
            context.HttpContext.Response.StatusCode = 500;
            context.Result = new JsonResult(ExceptionPayload.New(context.Exception));
        }
    }
}
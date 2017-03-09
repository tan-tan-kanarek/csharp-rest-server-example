using ServerExample.Errors;
using ServerExample.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;

namespace ServerExample.App
{
    public class ErrorHandler : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            Exception exception = context.Exception;
            if (exception != null && exception is RestException)
            {
                RestResponse response = new RestResponse(null, new RestError(exception as RestException));
                context.Response = context.Request.CreateResponse(HttpStatusCode.OK, response);
            }

            base.OnException(context);
        }
    }
}
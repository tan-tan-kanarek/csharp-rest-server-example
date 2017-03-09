using csharp_rest_server_example.Controllers;
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
using System.Web.Http;

namespace ServerExample.App
{
    public class WrappingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //let other handlers process the request
            var response = await base.SendAsync(request, cancellationToken);
            var wrappedResponse = BuildApiResponse(request, response);
            return wrappedResponse;
        }

        private static HttpResponseMessage BuildApiResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            object result = null;
            RestError error = null;

            response.TryGetContentValue(out result);

            if (result != null && result is HttpError)
            {
                error = new RestError(new RestSystemException(((HttpError)result).Message));
                result = null;
            }
            else if (result is RestResponse)
            {
                return response;
            }
            else if (result is SchemeController.Scheme)
            {
                return response;
            }

            HttpResponseMessage newResponse = request.CreateResponse(HttpStatusCode.OK, new RestResponse(result, error));

            foreach (var header in response.Headers)
            {
                newResponse.Headers.Add(header.Key, header.Value);
            }

            return newResponse;
        }
    }
}
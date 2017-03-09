using csharp_rest_server_example.Controllers;
using Newtonsoft.Json.Linq;
using ServerExample.Errors;
using ServerExample.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ServerExample.App
{
    public class RequestParser : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // Rewind request stream
            Stream reqStream = actionContext.Request.Content.ReadAsStreamAsync().Result;
            if (reqStream.CanSeek)
            {
                reqStream.Position = 0;
            }

            // Read the content as string
            string result = actionContext.Request.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(result))
            {
                return;
            }

            List<HttpParameterDescriptor> propertiesDescriptors = actionContext.ActionDescriptor.ActionBinding.ParameterBindings.Select(parameterBinding => parameterBinding.Descriptor).ToList();
            
            using (var input = new StringReader(result))
            {
                try
                {
                    JObject jObject = JObject.Parse(input.ReadToEnd());
                    Dictionary<string, object> outputArguments;
                    if (actionContext.ControllerContext.Controller is MultirequestController)
                    {
                        outputArguments = new Dictionary<string, object>();
                        outputArguments.Add("requests", DeserializedMultiRequest((MultirequestController) actionContext.ControllerContext.Controller, jObject));
                    }
                    else
                    {
                        outputArguments = DeserializeActionParameters(jObject, propertiesDescriptors);
                    }

                    foreach (string key in outputArguments.Keys)
                    {
                        actionContext.ActionArguments.Remove(key);
                        actionContext.ActionArguments.Add(key, outputArguments[key]);
                    }
                }
                catch (FormatException)
                {
                    throw new RestRequestException(RestRequestException.INVALID_JSON);
                }
            }
        }

        private List<RestRequest> DeserializedMultiRequest(MultirequestController multirequestController, JObject jObject)
        {
            List<RestRequest> requests = new List<RestRequest>();

            int index = 1;
            while (jObject[index.ToString()] != null)
            {
                JObject jRequest = (JObject) jObject[index.ToString()];
                RestRequest request = new RestRequest();
                request.Service = jRequest["service"].ToString();
                request.Action = jRequest["action"].ToString();
                request.Arguments = new Dictionary<string, object>();

                MethodInfo methodInfo = multirequestController.GetMethodInfo(request.Service, request.Action);
                foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
                {
                    string name = parameterInfo.Name;

                    JToken jToken = jRequest[name];
                    if (jToken == null || jToken.Type == JTokenType.Null)
                    {
                        if (!parameterInfo.IsOptional)
                        {
                            throw new RestRequestException(RestRequestException.MISSING_PARAMETER, name);
                        }
                    }
                    else if (jToken.Type == JTokenType.String && MultirequestController.MultirequestRegex.IsMatch((string)jToken))
                    {
                        request.Arguments.Add(name, (string) jToken);
                    }
                    else
                    {
                        request.Arguments.Add(name, RestObject.FromJson(parameterInfo.ParameterType, jToken));
                    }
                }

                requests.Add(request);
                index++;
            }

            return requests;
        }

        private Dictionary<string, object> DeserializeActionParameters(JObject jObject, List<HttpParameterDescriptor> propertiesDescriptors)
        {
            Dictionary<string, object> outputArguments = new Dictionary<string, object>();
            foreach (HttpParameterDescriptor propertyDescriptor in propertiesDescriptors)
            {
                string name = propertyDescriptor.ParameterName;

                JToken jToken = jObject[name];
                if (jToken == null || jToken.Type == JTokenType.Null)
                {
                    if (!propertyDescriptor.IsOptional)
                    {
                        throw new RestRequestException(RestRequestException.MISSING_PARAMETER, name);
                    }
                }
                else
                {
                    outputArguments.Add(name, RestObject.FromJson(propertyDescriptor.ParameterType, jToken));
                }
            }

            return outputArguments;
        }
    }
}
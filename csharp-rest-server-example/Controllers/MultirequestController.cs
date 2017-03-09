using ServerExample;
using ServerExample.App;
using ServerExample.Errors;
using ServerExample.Model;
using ServerExample.Model.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using System.Web.Http.ValueProviders;
using System.Web.UI;

namespace csharp_rest_server_example.Controllers
{
    public class MultirequestController : ApiController
    {
        public static Regex MultirequestRegex = new Regex(@"^{results:(\d):?(.*)}$");

        class WrongNumberOfTokensException : Exception
        {
        }

        [HttpPost]
        public RestResponseList Handle([ValueProvider] List<RestRequest> requests)
        {
            RestResponseList responses = new RestResponseList();

            foreach (RestRequest request in requests)
            {
                HttpControllerContext controllerContext = GetControllerContext(request.Service, request.Action);
                HttpActionDescriptor actionDescriptor = GetActionDescriptor(controllerContext);
                MethodInfo methodInfo = GetMethodInfo(actionDescriptor);

                object[] arguments = ReplaceTokens(request.Arguments, responses).Values.ToArray();

                RestResponse response;
                try
                {
                    object result = methodInfo.Invoke(GetController(controllerContext.ControllerDescriptor.ControllerType), arguments);
                    response = new RestResponse(result, null);
                }
                catch (Exception e)
                {
                    RestException exception;
                    if (e.InnerException != null)
                    {
                        if (e.InnerException is RestException)
                        {
                            exception = (RestException)e.InnerException;
                        }
                        else
                        {
                            exception = new RestSystemException(e.InnerException.Message);
                        }
                    }
                    else
                    {
                        exception = new RestSystemException(e.Message);
                    }

                    response = new RestResponse(null, new RestError(exception));
                }
                responses.Add(response);
            }

            return responses;
        }

        [NonAction]
	    public object ReplaceToken(object value, List<RestResponse> responses)
	    {
		    if(value is Dictionary<string, object>)
		    {
			    return ReplaceTokens((Dictionary<string, object>) value, responses);
		    }

		    else if(value is List<object>)
		    {
			    return ReplaceTokens((List<object>) value, responses);
		    }

            else if (value is string && MultirequestRegex.IsMatch((string)value))
		    {
			    String token = (string)value;

                Match match = MultirequestRegex.Match(token);
                int responseIndex = Int32.Parse(match.Groups[1].Value) - 1;
				if(responseIndex >= responses.Count())
				{
					throw new RestRequestException(RestRequestException.INVALID_MULTIREQUEST_TOKEN, token);
				}

                ;
                SortedList<int, string> tokens = new SortedList<int, string>(match.Groups[2].Value.Split(':').Select((v, i) => new { Key = i, Value = v }).ToDictionary(o => o.Key, o => o.Value));
                try
                {
                    return ReplaceToken(tokens, responses[responseIndex].JsonResult);
                }
                catch (WrongNumberOfTokensException)
                {
                    throw new RestRequestException(RestRequestException.INVALID_MULTIREQUEST_TOKEN, token);
                }
		    }

            return value;
        }

        [NonAction]
        private object ReplaceToken(SortedList<int, string> tokens, object result)
        {
		    if(tokens.Count <= 0)
		    {
			    return result;
		    }
		
		    string token = tokens.First().Value;
            tokens.RemoveAt(0);

		    if(result is Dictionary<string, object>)
		    {
			    Dictionary<string, Object> map = (Dictionary<string, Object>)result;
			    if(map.ContainsKey(token))
                {
				    return ReplaceToken(tokens, map[token]);
                }
		    }

            int index;
		    if(result is List<object> && Int32.TryParse(token, out index))
		    {
			    List<object> list = (List<object>)result;
			    if(index < list.Count)
                {
				    return ReplaceToken(tokens, list[index]);
                }
		    }

		    Type type = result.GetType();
		    foreach(PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
		    {
                DataMemberAttribute dataMemberAttribute = property.GetCustomAttribute<DataMemberAttribute>();
                if(dataMemberAttribute == null || !dataMemberAttribute.Name.Equals(token))
                {
				    continue;
                }
			
			    return ReplaceToken(tokens, property.GetValue(result));
		    }
			
		    throw new WrongNumberOfTokensException();
        }

        [NonAction]
        private List<object> ReplaceTokens(List<object> list, List<RestResponse> responses)
        {
            List<object> values = new List<object>();
            foreach (object value in list)
            {
                values.Add(ReplaceToken(value, responses));
            }

            return values;
        }

        [NonAction]
        private Dictionary<string, object> ReplaceTokens(Dictionary<string, object> arguments, List<RestResponse> responses)
        {
            if (responses.Count() == 0)
            {
                return arguments;
            }

            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (string key in arguments.Keys)
            {
                values.Add(key, ReplaceToken(arguments[key], responses));
            }

            return values;
        }

        [NonAction]
        private HttpRequestMessage GetRequestMessage(string service, string action)
        {
            string uri = string.Format("/api/service/{0}/action/{1}", service, action);
            Uri absoluteUri = new Uri(Request.RequestUri, uri);

            return new HttpRequestMessage(HttpMethod.Post, absoluteUri.ToString());
        }

        [NonAction]
        private HttpControllerContext GetControllerContext(string service, string action)
        {
            HttpRequestMessage requestMessage = GetRequestMessage(service, action);
            IHttpRouteData routeData = Configuration.Routes.GetRouteData(requestMessage);
            HttpControllerDescriptor controllerDescriptor = SelectController(service);
            HttpControllerContext controllerContext = new HttpControllerContext(Configuration, routeData, requestMessage);
            controllerContext.ControllerDescriptor = controllerDescriptor;
            return controllerContext;
        }

        [NonAction]
        private HttpActionDescriptor GetActionDescriptor(string service, string action)
        {
            HttpControllerContext controllerContext = GetControllerContext(service, action);
            return GetActionDescriptor(controllerContext);
        }

        [NonAction]
        private HttpActionDescriptor GetActionDescriptor(HttpControllerContext controllerContext)
        {
            ApiControllerActionSelector actionSelector = new ApiControllerActionSelector();
            return actionSelector.SelectAction(controllerContext);
        }

        [NonAction]
        public MethodInfo GetMethodInfo(string service, string action)
        {
            HttpActionDescriptor actionDescriptor = GetActionDescriptor(service, action);
            return GetMethodInfo(actionDescriptor);
        }

        [NonAction]
        private MethodInfo GetMethodInfo(HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor is ReflectedHttpActionDescriptor)
            {
                return ((ReflectedHttpActionDescriptor)actionDescriptor).MethodInfo;
            }

            return null;
        }

        [NonAction]
        private ApiController GetController(Type controllerType)
        {
            return (ApiController)Activator.CreateInstance(controllerType);
        }

        [NonAction]
        private HttpControllerDescriptor SelectController(string controllerName)
        {
            string className = string.Format("{0}Controller", controllerName.Substring(0, 1).ToUpper() + controllerName.Substring(1));

            Assembly assembly = Assembly.GetExecutingAssembly();
            Type controllerType = assembly.GetTypes().Where(type => type.Name == className).First();

            return new HttpControllerDescriptor(Configuration, controllerName, controllerType);
        }
    }
}
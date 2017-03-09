using ServerExample.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerExample.Errors
{
    public class RestException : Exception
    {
        public class RestExceptionType
        {
            public string Code { get; set; }
            private string Template { get; set; }
            private string[] ParameterNames { get; set; }

            public RestDictionary<string> getParameters(params string[] parameters)
            {
                if (ParameterNames == null || ParameterNames.Length == 0)
                    return null;

                RestDictionary<string> map = new RestDictionary<string>();
                for (int i = 0; i < ParameterNames.Count(); i++)
                {
                    map.Add(ParameterNames[i], parameters[i]);
                }

                return map;
            }

            public string Format(params string[] parameters)
            {
                if (ParameterNames == null || ParameterNames.Length == 0)
                    return Template;

                string ret = Template;
                string token;

                for (int i = 0; i < ParameterNames.Count(); i++ )
                {
                    token = string.Format("@{0}@", ParameterNames[i]);
                    ret = ret.Replace(token, parameters[i]);
                }

                return ret;
            }

            public RestExceptionType(string code, string template, params string[] parameters)
            {
                Code = code;
                Template = template;
                ParameterNames = parameters;
            }
        }

        public string Code { get; set; }

        public RestDictionary<string> Parameters { get; protected set; }

        protected RestException(RestExceptionType type, params string[] parameters)
            : base(type.Format(parameters))
        {
            Code = type.Code;
            Parameters = type.getParameters(parameters);
        }

        protected RestException(RestExceptionType type, string messaage)
            : base(messaage)
        {
            Code = type.Code;
        }
    }
}
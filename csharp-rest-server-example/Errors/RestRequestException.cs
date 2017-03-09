using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerExample.Errors
{
    public class RestRequestException : RestException
    {
        public static RestExceptionType MISSING_PARAMETER = new RestExceptionType("MISSING_PARAMETER", "Argument [@argument@] is missing", "argument");
        public static RestExceptionType INVALID_JSON = new RestExceptionType("INVALID_JSON", "Invalid JSON format");
        public static RestExceptionType INVALID_MULTIREQUEST_TOKEN = new RestExceptionType("INVALID_MULTIREQUEST_TOKEN", "Invalid multi-request token [@token@]", "token");

        public RestRequestException(RestExceptionType type, params string[] parameters)
            : base(type, parameters)
        {
        }
    }
}
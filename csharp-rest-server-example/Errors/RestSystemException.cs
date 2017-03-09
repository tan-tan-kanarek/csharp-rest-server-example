using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerExample.Errors
{
    public class RestSystemException : RestException
    {
        public static RestExceptionType INTERNAL_SERVER_ERROR = new RestExceptionType("INTERNAL_SERVER_ERROR", "Internal server error");

#if DEBUG
        public RestSystemException(string message)
            : base(INTERNAL_SERVER_ERROR, message)
        {
        }
#else
        public RestSystemException(string message)
            : base(INTERNAL_SERVER_ERROR)
        {
        }
#endif
    }
}
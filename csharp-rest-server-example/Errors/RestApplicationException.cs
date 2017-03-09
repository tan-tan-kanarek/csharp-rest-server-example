using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerExample.Errors
{
    public class RestApplicationException : RestException
    {
        public static RestExceptionType OBJECT_NOT_FOUND = new RestExceptionType("OBJECT_NOT_FOUND", "@type@ id [@id@] not found", "type", "id");

        public RestApplicationException(RestExceptionType type, params string[] parameters)
            : base(type, parameters)
        {
        }
    }
}
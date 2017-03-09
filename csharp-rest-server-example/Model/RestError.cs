using ServerExample.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;

namespace ServerExample.Model
{
    [DataContract(Namespace = "")]
    public class RestError
    {
        public RestError(RestException exception)
        {
            Code = exception.Code;
            Message = exception.Message;
            Parameters = exception.Parameters;
        }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "parameters")]
        public RestDictionary<string> Parameters { get; set; }
    }
}
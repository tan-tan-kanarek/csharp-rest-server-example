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
    public class RestRequest
    {
        [DataMember(Name = "service")]
        public string Service { get; set; }

        [DataMember(Name = "action")]
        public string Action { get; set; }

        public Dictionary<string, object> Arguments { get; set; }
    }
}
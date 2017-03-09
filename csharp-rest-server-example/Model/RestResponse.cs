using Newtonsoft.Json;
using ServerExample.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;
using System.Xml.Serialization;

namespace ServerExample.Model
{
    [DataContract(Name = "xml", Namespace = "")]
    public class RestResponse
    {
        public RestResponse(object result, RestError error)
        {
            if (result is IRestObject)
            {
                JsonResult = ((IRestObject)result).ToJson();
            }
            else
            {
                JsonResult = result;
            }

            Error = error;
            XmlResult = result;
        }

        [DataMember(Name = "result", EmitDefaultValue = false)]
        [JsonIgnore]
        public object XmlResult { get; set; }

        [JsonProperty(PropertyName = "result")]
        [XmlIgnore]
        public object JsonResult { get; set; }

        [DataMember(Name = "error", EmitDefaultValue = false)]
        public RestError Error { get; set; }
    }
}
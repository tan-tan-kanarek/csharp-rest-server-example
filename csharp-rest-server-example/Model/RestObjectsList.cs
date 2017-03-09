using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace ServerExample.Model
{
    [DataContract(Namespace = "")]
    public class RestObjectsList<T> : RestObject where T : RestObject
    {
        public RestObjectsList()
        {
            Objects = new RestList<T>();
        }

        [DataMember(Name = "objects")]
        public RestList<T> Objects { get; set; }

        [DataMember(Name = "totalCount")]
        public long TotalCount { get; set; }
    }
}
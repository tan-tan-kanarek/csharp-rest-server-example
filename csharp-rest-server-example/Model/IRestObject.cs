using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;

namespace ServerExample.Model
{
    public interface IRestObject
    {
        Dictionary<string, object> ToJson();
    }
}
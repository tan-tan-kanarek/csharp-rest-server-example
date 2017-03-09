using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace ServerExample.Model
{
    [DataContract(Namespace = "")]
    public enum UserStatus
    {
        [EnumMember(Value = "0")] ACTIVE,
        [EnumMember(Value = "1")] DISABLED
    }
}
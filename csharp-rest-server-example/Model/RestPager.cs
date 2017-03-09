using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ServerExample.Model
{
    [DataContract(Namespace = "")]
    public class RestPager : RestObject
    {
        public RestPager()
        {
            PageIndex = 1;
            PageSize = 500;
        }

        [DataMember(Name = "pageIndex")]
        public int PageIndex { get; set; }

        [DataMember(Name = "pageSize")]
        public int PageSize { get; set; }
    }
}
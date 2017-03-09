using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ServerExample.Model.Filters
{
    [RestTable("users")]
    public class UserFilter : RestFilter<User>
    {
        [DataMember(Name = "createdAtGreaterThanOrEqual")]
        [RestConditionAttribute("created_at", Operator = ">=")]
	    public RestDateTime CreatedAtGreaterThanOrEqual { get; set; }

        [DataMember(Name = "createdAtLessThanOrEqual")]
        [RestConditionAttribute("created_at", Operator = "<=")]
        public RestDateTime CreatedAtLessThanOrEqual { get; set; }

        [DataMember(Name = "updatedAtGreaterThanOrEqual")]
        [RestConditionAttribute("updated_at", Operator = ">=")]
        public RestDateTime UpdatedAtGreaterThanOrEqual { get; set; }

        [DataMember(Name = "updatedAtLessThanOrEqual")]
        [RestConditionAttribute("updated_at", Operator = "<=")]
        public RestDateTime UpdatedAtLessThanOrEqual { get; set; }
    }
}
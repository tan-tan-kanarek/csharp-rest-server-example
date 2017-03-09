using ServerExample.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace ServerExample.Model
{
    [DataContract(Namespace = "")]
    [RestTable("users")]
    public class User : RestPersistentObject
    {
        public User()
            : base()
        {
        }

        public User(long id)
            : base(id)
        {
        }

        protected override void SetDefaults()
        {
            base.SetDefaults();
            Status = UserStatus.ACTIVE;
        }

        [DataMember(Name = "firstName")]
        [RestColumn("first_name")]
	    public string FirstName { get; set; }

        [DataMember(Name = "lastName")]
        [RestColumn("last_name")]
	    public string LastName { get; set; }

        [DataMember(Name = "status")]
	    public UserStatus? Status { get; set; }
    }
}
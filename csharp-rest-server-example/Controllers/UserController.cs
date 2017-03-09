using ServerExample;
using ServerExample.Errors;
using ServerExample.Model;
using ServerExample.Model.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ValueProviders;

namespace csharp_rest_server_example.Controllers
{
    public class UserController : ApiController, IRestController
    {
        [ActionName("add"), HttpPost]
        public User Add([ValueProvider] User user)
        {
            return user.Add<User>();
        }

        [ActionName("get"), HttpPost]
        public User Get([ValueProvider] long id)
        {
            return new User(id);
        }

        [ActionName("update"), HttpPost]
        public User Update([ValueProvider] long id, [ValueProvider] User user)
	    {
            User existingUser = new User(id);
            return existingUser.Update<User>(user);
	    }

        [ActionName("delete"), HttpPost]
        public void Delete([ValueProvider] long id)
        {
            User existingUser = new User(id);
            existingUser.Delete();
        }

        [ActionName("search"), HttpPost]
        public UsersList Search([ValueProvider] UserFilter filter = null, [ValueProvider] RestPager pager = null)
        {
		    if(filter == null)
			    filter = new UserFilter();

		    if(pager == null)
                pager = new RestPager();

            return filter.Search<UsersList>(pager);
	    }
    }
}
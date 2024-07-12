using HTTP.Library.models;
using System.Collections.Generic;
using System.Net;

namespace HTTP.Library.services
{
    public interface ICookie_Action
    {
        void Clear(string name_of_cookie);
        List<item> Get_Cookie(string name_of_cookie, CookieContainer cookies);
    }
}

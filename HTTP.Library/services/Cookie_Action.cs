using HTTP.Library.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace HTTP.Library.services
{
    internal class Cookie_Action: ICookie_Action
    {
        public void Clear(string name_of_cookie)
        {
            CookieContainer c = new CookieContainer();
            var cookies = c.GetCookies(new Uri(name_of_cookie));
            foreach (Cookie co in cookies)
            {
                co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            }
        }

        public List<item> Get_Cookie(string name_of_cookie, CookieContainer cookies)
        {
            List<item> res = new List<item>();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            Uri uri = new Uri(name_of_cookie);
            IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
            foreach (Cookie cookie in responseCookies) res.Add(new item(cookie.Name, cookie.Value));

            return res;
        }
    }
}

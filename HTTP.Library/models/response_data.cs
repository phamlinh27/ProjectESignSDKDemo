using System.Net;
namespace HTTP.Library.models
{
    public class response_data <T>
    {
        public int code { get; set; }
        public string message { get; set; }
        public T data { get; set; }
        public CookieContainer cookies { get; set; }
    }
}

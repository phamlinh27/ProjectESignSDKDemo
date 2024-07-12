using HTTP.Library.services;

namespace HTTP.Library.actions
{
    public class HTTP_Call_Action
    {
        public IRESTful_Action RESTful_Action { get; set; }
        public ICookie_Action Cookie_Action { get; set; }
        public HTTP_Call_Action() {
            RESTful_Action = new RESTful_Action();
            Cookie_Action = new Cookie_Action();
        }
    }
}

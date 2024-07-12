using HTTP.Library.actions;
using HTTP.Library.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectESignSDKDemo
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Configuration = System.Configuration.ConfigurationSettings.AppSettings;
            HTTP_Call_Action http_call_action = new HTTP_Call_Action();
            RESTful_Action = http_call_action.RESTful_Action;
            Cookie_Action = http_call_action.Cookie_Action;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmLogin());
            
        }

        public static string TOKEN { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static IRESTful_Action RESTful_Action { get; set; }
        public static ICookie_Action Cookie_Action { get; set; }

        public static System.Collections.Specialized.NameValueCollection Configuration { get; set; }
    }
}

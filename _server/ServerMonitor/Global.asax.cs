using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;
using MediatR;
using System.Web.Http;
using ServerMonitor.Helpers;

namespace ServerMonitor
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Bootstrapper.Run();
        }
    }
}

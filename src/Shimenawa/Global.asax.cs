using System.Web.Http;
using Unity.WebApi;

namespace Medidata.Shimenawa
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // set dependency injection to use Unity for Web API (ApiController based)
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(UnityConfig.GetConfiguredContainer());
        }
    }
}

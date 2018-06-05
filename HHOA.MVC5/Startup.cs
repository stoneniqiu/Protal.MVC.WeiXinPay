using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Portal.MVC5.Startup))]
namespace Portal.MVC5
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

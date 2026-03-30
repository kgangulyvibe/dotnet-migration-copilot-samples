using Microsoft.Identity.Web;
using Microsoft.Identity.Web.OWIN;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ContosoUniversity.Startup))]
namespace ContosoUniversity
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            OwinTokenAcquirerFactory factory = TokenAcquirerFactory.GetDefaultInstance<OwinTokenAcquirerFactory>();
            app.AddMicrosoftIdentityWebApi(factory);

            factory.Build();
        }
    }
}

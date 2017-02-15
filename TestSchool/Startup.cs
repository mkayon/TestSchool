using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TestSchool.Startup))]
namespace TestSchool
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

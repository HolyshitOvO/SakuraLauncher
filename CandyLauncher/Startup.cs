using Hake.Extension.DependencyInjection.Abstraction;
using CandyLauncher.Abstraction.Action;
using CandyLauncher.Abstraction.Base;
using CandyLauncher.Implementation.Components.PluginLoader;
using CandyLauncher.Implementation.Components.ErrorBlocker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CandyLauncher.Implementation.Services.Tray;
//using Chrome.BookmarkSearch;

namespace CandyLauncher
{
    public sealed class Startup
    {
        public Startup()
        {

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPluginProvider();
        }
        public void ConfigureComponents(IAppBuilder app)
        {
            // 注释了这里
            // bookmark search should not use parsed input arguments
            // so UseErrorBlocker must be put behind
            //app.UseChromeBookmarkSearch();

            //app.UseErrorBlocker(blockIfError: true);

            app.UsePlugins();
        }
    }
}

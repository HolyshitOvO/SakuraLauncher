using CandyLauncher.Abstraction.Services;
using CandyLauncher.Abstraction.Base;
using Hake.Extension.DependencyInjection.Abstraction;
using CandyLauncher.Implementation.Services.CurrentEnvironment;

namespace CandyLauncher.Implementation.Base
{
    public static class CurrentEnvironmentExtensions
    {
        public static IHostBuilder UseEnvironment(this IHostBuilder builder, string plugin, string config, string log)
        {
            builder.ConfigureService(services =>
            {
                IServiceCollection pool = services.GetService(typeof(IServiceCollection)) as IServiceCollection;
                ICurrentEnvironment currenv = new CurrentEnvironment(plugin, config, log);
                pool.Add(ServiceDescriptor.Singleton<ICurrentEnvironment>(currenv));
            });
            return builder;
        }
    }
}

using Hake.Extension.DependencyInjection.Abstraction;
using CandyLauncher.Abstraction.Base;
using System;

namespace CandyLauncher.Implementation.Components.PluginLoader
{
    public static class PluginLoaderExtensions
    {
        public static IServiceCollection AddPluginProvider(this IServiceCollection pool)
        {
            IServiceProvider services = pool.GetDescriptor<IServiceProvider>().GetInstance() as IServiceProvider;
            IPluginProvider provider = services.CreateInstance<PluginProvider>();
            pool.Add(ServiceDescriptor.Singleton<IPluginProvider>(provider));
            return pool;
        }
        public static IAppBuilder UsePlugins(this IAppBuilder builder)
        {
            return builder.UseComponent<PluginLoader>();
        }
    }
}

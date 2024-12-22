using System;

using Hake.Extension.DependencyInjection.Abstraction;
using CandyLauncher.Abstraction.Base;
using CandyLauncher.Implementation.Services.Tray;
using CandyLauncher.Implementation.Services.ProgramContext;
using CandyLauncher.Abstraction.Services;
using CandyLauncher.Implementation.Services.TerminationNotifier;
using System.Windows.Forms;
using CandyLauncher.Implementation.Services.HotKey;
using CandyLauncher.Implementation.Services.Logger;
using Hake.Extension.DependencyInjection;

namespace CandyLauncher.Implementation.Base
{
    public class HostBuilder : IHostBuilder
    {
        private IServiceProvider services;
        private IServiceCollection pool;
        private IAppBuilder app;

        public HostBuilder()
        {
            pool = Hake.Extension.DependencyInjection.Implementation.CreateServiceCollection();
            services = pool.CreateProvider();
            pool.Add(ServiceDescriptor.Singleton<IServiceCollection>(pool));
            pool.Add(ServiceDescriptor.Singleton<IServiceProvider>(services));

            ConfigureInternalServices();

            app = services.CreateInstance<AppBuilder>();
            pool.Add(ServiceDescriptor.Singleton<IAppBuilder>(app));
        }
        
        public IHost Build()
        {
            string log = "";
            if (services.TryGetService<IHotKeyBuilder>(out IHotKeyBuilder hotkeyBuilder) == false)
            {
                log += "hotkey not configured";
                MessageBox.Show(log);
            }
            IHotKey hotkey = hotkeyBuilder.Build();
            pool.Remove(pool.GetDescriptor<IHotKeyBuilder>());
            pool.Add(ServiceDescriptor.Singleton<IHotKey>(hotkey));
            return services.CreateInstance<Host>();
        }

        public IHostBuilder ConfigureService(Action<IServiceProvider> configureServices)
        {
            if (configureServices == null)
                throw new ArgumentNullException(nameof(configureServices));
            configureServices.Invoke(services);
            return this;
        }

        private void ConfigureInternalServices()
        {
            ITerminationNotifier terminationNotifier = services.CreateInstance<TerminationNotifier>();
            pool.Add(ServiceDescriptor.Singleton<ITerminationNotifier>(terminationNotifier));

            ITray tray = services.CreateInstance<Tray>();
            pool.Add(ServiceDescriptor.Singleton<ITray>(tray));

            IProgramContextFactory programContextFactory = services.CreateInstance<ProgramContextFactory>();
            pool.Add(ServiceDescriptor.Singleton<IProgramContextFactory>(programContextFactory));
            pool.Add(ServiceDescriptor.Scoped<IProgramContext>(services =>
            {
                IProgramContextFactory factory = services.GetService<IProgramContextFactory>();
                return factory.RebuildContext();
            }));
        }
    }
}

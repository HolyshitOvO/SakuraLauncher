using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CandyLauncher.Abstraction.Base
{
    public interface IHostBuilder
    {
        IHost Build();
        IHostBuilder ConfigureService(Action<IServiceProvider> configureServices);
    }
}

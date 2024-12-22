using Hake.Extension.ValueRecord;
using Hake.Extension.ValueRecord.Mapper;
using HakeQuick.Abstraction.Base;
using HakeQuick.Implementation.Base;
using HakeQuick.Implementation.Configuration;
using HakeQuick.Implementation.Services.HotKey;
using HakeQuick.Implementation.Services.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HakeQuick
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddDefault()
                .TryAddJson("settings.json")
                .Build();
            // 更改配置文件存放位置，以及log文件存放位置
            string myExePath = Helpers.Tools.GetApplicationFolderPath();
            configuration.Options.ConfigPath = myExePath;
            configuration.Options.LogPath = myExePath;
            QuickConfig options = configuration.Options;

            IHost host = new HostBuilder()
                .AddConfiguration(configuration)
                .UseEnvironment(plugin: "plugins", config: options.ConfigPath, log: options.LogPath)
                .AddFileLoggerFactory()
                .UseHotKey(key: options.Hotkey.Key, flags: options.Hotkey.KeyFlags)
                .UseWindow<DefaultWindow>()
                .UseConfiguration<Startup>()
                .Build();
            host.Run();
        }
    }
}

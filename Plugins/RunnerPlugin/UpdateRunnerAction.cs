using HakeQuick.Abstraction.Action;
using HakeQuick.Abstraction.Services;
using HakeQuick.Helpers;
using System;
using System.Reflection;

namespace RunnerPlugin
{
    /// <summary>
    /// 内置的更新配置的列表 item 项
    /// </summary>
    internal sealed class UpdateRunnerAction : ActionBase
    {
        public UpdateRunnerAction()
        {
            Title = "刷新运行";
            Subtitle = "重新加载运行配置";
            Icon = Assembly.GetExecutingAssembly().LoadImage("RunnerPlugin.Resources.reload.png");
            IsExecutable = true;
        }

        public void Invoke(ICurrentEnvironment env)
        {
            ListedRunnerPlugin.Instance?.UpdateConfigurations(env);
        }
    }
}

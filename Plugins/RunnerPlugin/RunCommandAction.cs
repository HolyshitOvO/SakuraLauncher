// 索引
// WindowsThumbnailProvider

using HakeQuick.Abstraction.Action;
using HakeQuick.Abstraction.Base;
using HakeQuick.Abstraction.Services;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace RunnerPlugin
{
    internal sealed class RunCommandAction : ActionBase
    {
        /// <summary>
        /// 是否以管理员运行
        /// </summary>
        private readonly bool defaultAsAdmin;
        /// <summary>
        /// 工作目录
        /// </summary>
        private readonly string workingDirectory;
        /// <summary>
        /// 运行命令，附加的参数
        /// </summary>
        private readonly string arguments;
        /// <summary>
        /// 最终的运行命令
        /// </summary>
        public readonly string RunCommand;
        /// <summary>
        /// 运行命令
        /// </summary>
        private readonly string commandPath;

        /// <summary>
        /// 快捷方式路径
        /// </summary>
        private readonly string lnkPath;

        /// <summary>
        /// 对 item 相关参数，稍加处理
        /// </summary>
        /// <param name="run"></param>
        /// <param name="path"></param>
        /// <param name="iconPath"></param>
        /// <param name="admin"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="args"></param>
        public RunCommandAction(string run, string path, string iconPath, bool admin, string workingDirectory, string argsStr, string lnkPath = null)
        {
            // TODO: 简化args 传递，json里不要使用数组
            defaultAsAdmin = admin;
            this.workingDirectory = workingDirectory;
            this.lnkPath = lnkPath;
            arguments = argsStr ?? "";
            string appendArgs = string.IsNullOrEmpty(arguments) ? "" : " " + arguments;
            path = path ?? "";
            path = path.Trim();
            commandPath = path;
            IsExecutable = true;
            Title = run;
            if (path.Length > 0)
                Subtitle = path + appendArgs;
            else
                Subtitle = run + appendArgs;
            // 往索引字符串，后面添加拼音索引，即：计算器jisuanqi
            RunCommand = PinyinHelper.GetPinyinLongStr(run.ToLower());
            Debug.WriteLine(RunCommand);
            // 显示的图标
            if (iconPath != null)
            {
                try
                {
                    Icon = new BitmapImage(new Uri(iconPath));
                }
                catch
                {
                    Icon = null;
                }
            }
            else
            {
                // 获取文件图标
                //Icon = SystemIcon.ToBitmapImage(Image.FromHbitmap(SystemIcon.GetIcon(path, false).ToBitmap().GetHbitmap()));//测试
                // 判断是否为快捷方式，若是直接就获取lnk文件的图标即可
                string tempIconPath = string.IsNullOrEmpty(lnkPath) ? path : lnkPath;
                try
                {
                    // 某些情况会闪退
                    //Icon = SystemIcon.ToBitmapImage(WindowsThumbnailProvider.GetThumbnail(tempIconPath, 48, 48, ThumbnailOptions.IconOnly));
                    Icon = SystemIcon.ToBitmapImage(WindowsThumbnailProvider.GetThumbnail(tempIconPath, 48, 48, ThumbnailOptions.None));
                }
                catch (Exception)
                {
                    Icon = SystemIcon.ToBitmapImage(SystemIcon.GetIcon(tempIconPath, true).ToBitmap());// 这个也可以，但是某些图标不行
                }
            }
        }

        /// <summary>
        /// 窗口 Enter 时，运行的条目，在这里执行
        /// </summary>
        /// <param name="progContext"></param>
        /// <param name="runnerLogger"></param>
        /// <param name="command"></param>
        /// <param name="admin">是否以管理员运行</param>
        /// <param name="a"></param>
        public void Invoke(IProgramContext progContext, ILogger runnerLogger, ICommand command, bool admin = false, bool a = false)
        {
            // 如果它是快捷方式，那么就直接执行快捷方式，不用后面的方法
            if (!string.IsNullOrEmpty(lnkPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = lnkPath, UseShellExecute = true };
                Process.Start(startInfo);
                return;
            }
            admin = admin || a || defaultAsAdmin;
            string procname = RunCommand;
            if (commandPath.Length > 0)
                procname = commandPath;
            ProcessStartInfo psi = new ProcessStartInfo(procname);
            if (arguments != null)
                psi.Arguments = arguments;

            if (admin)
                psi.Verb = "runas";
            if (workingDirectory == null)
                // 获取当前工作目录
                psi.WorkingDirectory = Helper.CurrentWorkingDirectoryOrDefault(progContext);
            else
                psi.WorkingDirectory = workingDirectory;
            try
            {
                // 启动一个新进程，执行指定命令
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                runnerLogger.LogExceptionAsync(ex);
            }
        }
    }
}

// 索引
// WindowsThumbnailProvider

using HakeQuick.Abstraction.Action;
using HakeQuick.Abstraction.Base;
using HakeQuick.Abstraction.Services;
using System;
using System.Diagnostics;
using System.IO;
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
        /// 目标文件路径
        /// </summary>
        public readonly string targetFilePath;
        /// <summary>
        /// 是否为 UWP 项
        /// </summary>
        public readonly bool IsUwpItem = false;


        /// <summary>
        /// 对 item 相关参数，稍加处理
        /// </summary>
        /// <param name="justName"></param>
        /// <param name="targetFilePath"></param>
        /// <param name="iconPath"></param>
        /// <param name="admin"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="args"></param>
        public RunCommandAction(string justName, string targetFilePath, bool isUwpItem, BitmapImage iconBitmap = null, bool admin = false, string workingDirectory = null, string argsStr = null)
        {
            //Debug.WriteLine(run);
            if (isUwpItem)
            {
                defaultAsAdmin = true;
                arguments = "";
                IsExecutable = true;
                Subtitle = "UWP APP";

                IsUwpItem = true;
                RunCommand = PinyinHelper.GetPinyinLongStr(justName.ToLower());
                Title = justName;
                //Debug.WriteLine(RunCommand);
                this.targetFilePath = targetFilePath;
                Icon = iconBitmap;

                return;
            }
            // TODO: 简化args 传递，json里不要使用数组
            defaultAsAdmin = admin;
            this.workingDirectory = workingDirectory ?? Path.GetDirectoryName(targetFilePath);
            arguments = argsStr ?? "";
            string appendArgs = string.IsNullOrEmpty(arguments) ? "" : " " + arguments;
            targetFilePath = targetFilePath ?? "";
            targetFilePath = targetFilePath.Trim();
            this.targetFilePath = targetFilePath;
            IsExecutable = true;
            Title = justName;
            if (targetFilePath.Length > 0)
                Subtitle = targetFilePath + appendArgs;
            else
                Subtitle = justName + appendArgs;
            // 往索引字符串，后面添加拼音索引，即：计算器jisuanqi
            RunCommand = PinyinHelper.GetPinyinLongStr(justName.ToLower());
        }


        /// <summary>
        /// 窗口 使用快捷键Ctrl + o 时，打开所在文件夹位置
        /// </summary>
        public void InvokeOpenFolder()
        {
            // 如果它是快捷方式，那么就直接执行快捷方式，不用后面的方法
            // 打开目标路径所在的文件夹
            string folderPath = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(folderPath))
            {
                Process.Start("explorer.exe", "/select, " + targetFilePath);
            }
            return;

        }

        /// <summary>
        /// 窗口 使用快捷键Ctrl + i 时，打开所在文件夹位置
        /// </summary>
        public void InvokeOpenGoalFolder()
        {
            // 如果它是快捷方式，那么就直接执行快捷方式，不用后面的方法
            if (Path.GetExtension(targetFilePath) == ".lnk")
            {
                // 获取快捷方式的目标路径
                string targetPath = GetShortcutTarget(targetFilePath);

                // 打开目标路径所在的文件夹
                if (!string.IsNullOrEmpty(targetPath))
                {
                    string folderPath = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        Process.Start("explorer.exe", "/select, " + targetPath);
                        //Process.Start("explorer.exe", "/select, " + folderPath);
                    }
                }
                return;
            }
            else
            {
                InvokeOpenFolder();
            }
        }

        /// <summary>
        /// 获取快捷方式的目标路径
        /// </summary>
        static string GetShortcutTarget(string shortcutPath)
        {
            if (File.Exists(shortcutPath))
            {
                var shell = new IWshRuntimeLibrary.WshShell();
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                return shortcut.TargetPath;
            }
            return null;
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
            if (!IsUwpItem)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = targetFilePath, UseShellExecute = true };
                Process.Start(startInfo);
                return;
            }
            else
            {
                Process.Start("explorer.exe", targetFilePath);

                return;
                ProcessStartInfo psi2 = new ProcessStartInfo("explorer.exe")
                {
                    Verb = "runas",
                    WorkingDirectory = Helper.CurrentWorkingDirectoryOrDefault(progContext),
                    Arguments = targetFilePath
                };
                try
                {
                    // 启动一个新进程，执行指定命令
                    Process.Start(psi2);

                }
                catch (Exception ex)
                {
                    runnerLogger.LogExceptionAsync(ex);
                }
                return;
            }

            admin = admin || a || defaultAsAdmin;
            string procname = RunCommand;
            if (targetFilePath.Length > 0)
                procname = targetFilePath;
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

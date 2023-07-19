using HakeQuick.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunnerPlugin
{
    internal static class Helper
    {
        /// <summary>
        /// 获取当前程序的工作目录
        /// 或者如果程序不是资源管理器（Windows Explorer）进程，则返回默认目录。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="defaultDirectory"></param>
        /// <returns></returns>
        public static string CurrentWorkingDirectoryOrDefault(IProgramContext context, string defaultDirectory = "C:\\Windows\\System32")
        {
            if (context.CurrentProcess.ProcessName != "explorer")
                return defaultDirectory;

            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            int handle = context.WindowHandle.ToInt32();
            string path = null;
            foreach (SHDocVw.InternetExplorer exp in shellWindows)
            {
                if (handle == exp.HWND)
                {
                    path = System.Web.HttpUtility.UrlDecode(exp.LocationURL);
                    if (path.StartsWith("file:///") == true)
                        path = path.Substring(8);
                    if (path.Length > 0)
                        return path;
                    else
                        return defaultDirectory;
                }
            }
            path = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + "\\", Environment.GetEnvironmentVariable("HOMEPATH").Substring(1), "desktop");
            return path;
        }
    }
}

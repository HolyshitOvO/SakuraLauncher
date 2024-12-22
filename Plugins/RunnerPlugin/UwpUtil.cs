using CandyLauncher.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Drawing;

namespace RunnerPlugin
{
	internal static class UwpUtil
	{
        /// <summary>
        /// 完美获取可用 UWP 应用列表
        /// </summary>
        public static List<ShellObject> GetInstalledUWPApps()
        {
            // 缓存应用列表，如果列表已经缓存，可以避免重复执行 PowerShell 命令
            List<ShellObject> resultList = new List<ShellObject>();

            // 获取 UWP 应用名称的字典
            Dictionary<string, ShellObject> uwpApplicationsList = SpecificallyForGetCurrentUwpName();
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            // 如果只需要当前用户的应用，可以减少运行时间
            //process.StartInfo.Arguments = "Get-AppxPackage | Select-Object IsFramework,PackageFamilyName";
            //process.StartInfo.Arguments = "Get-AppxPackage | Where-Object { $_.IsFramework -eq $false } | Select-Object PackageFamilyName";
            process.StartInfo.Arguments = "Get-AppxPackage | Where-Object { $_.IsFramework -eq $false } | Select-Object PackageFamilyName | Format-Table -HideTableHeaders";
            process.Start();
            //string output = process.StandardOutput.ReadToEnd();
            //process.WaitForExit();

            // 使用流式读取，逐行处理输出
            using (StreamReader reader = process.StandardOutput)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if(uwpApplicationsList.ContainsKey(line)) 
                    resultList.Add(uwpApplicationsList[line]);
                }
            }
            //process.WaitForExit();
            // 清理字典
            //uwpApplicationsList.Clear();
            return resultList;
        }

        /// <summary>
        /// 字典，键为 UWP 的 FamilyName，值包含 Applications 文件夹里的所有项。
        /// 用途为获取 UWP 正确的应用名称、和判断是否为一般应用
        /// </summary>
        private static Dictionary<string, ShellObject> SpecificallyForGetCurrentUwpName()
		{
			Dictionary<string, ShellObject> specificallyForGetUwpName = new Dictionary<string, ShellObject>();
			ShellObject appsFolder = GetAppsFolder();

			foreach (ShellObject app in (IKnownFolder)appsFolder)
			{
				// appUserModelID 或应用程序。Properties.System.App用户模型.ID。您甚至可以在一次拍摄中获得Jumbo图标
				string appUserModelID = app.ParsingName;
				//System.Windows.Media.ImageSource icon = app.Thumbnail.ExtraLargeBitmapSource;
				// 剔除叹号后的内容
				if (appUserModelID.Contains('!'))
				{
					appUserModelID = appUserModelID.Substring(0, appUserModelID.LastIndexOf("!"));
				}
				specificallyForGetUwpName[appUserModelID] = app;
				//Console.WriteLine(app.Properties.System
				//System.Diagnostics.Process.Start("explorer.exe", @" shell:appsFolder\" + appUserModelID);
				//Process.Start（“explorer.exe”， @“shell：appsFolder\{ F38BF404 - 1D43 - 42F2 - 9305 - 67DE0B28FC23}\regedit.exe”）
			}
			return specificallyForGetUwpName;
		}
		/// <summary>
        /// 字典，键为 UWP 的 FamilyName，值包含 Applications 文件夹里的所有项。
        /// 用途为获取 UWP 正确的应用名称、和判断是否为一般应用
        /// </summary>
        internal static List<ShellObject> SpecificallyForGetCurrentUwpName2()
		{
            // 获取电脑 Applications 文件夹里面的项
            List<ShellObject> shellObjects = new List<ShellObject>();
			ShellObject appsFolder = GetAppsFolder();
			foreach (ShellObject app in (IKnownFolder)appsFolder)
			{
                shellObjects.Add(app);
			}

            List<ShellObject> resultList = new List<ShellObject>();
            // 从最后向前遍历，一般后面都为UWP应用
            for (int i = shellObjects.Count - 1; i >= 0; i--)
            {
                // 剔除叹号后的内容
                if (shellObjects[i].ParsingName.Contains('!'))
                {
                    resultList.Add(shellObjects[i]);
                }
                else
                {
                    break;
                }
            }
            return resultList;
        }

        /// <summary>
        /// 获取 Applications 里的所有应用，比较好的方法，能获取到图片
        /// </summary>
        /// <returns></returns>
		public static ShellObject GetAppsFolder()
		{
			// GUID taken from https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid
			var FODLERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
			ShellObject appsFolder = (ShellObject)KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder);
			return appsFolder;
		}

	}

}

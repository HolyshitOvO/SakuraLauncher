using HakeQuick.Abstraction.Services;
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
			Dictionary<string, ShellObject> uwpApplicationsList = SpecificallyForGetCurrentUwpName();
			List<ShellObject> resultList = new List<ShellObject>();

			// 使用 PS 来获取所有的 UWP 应用，但是名称不正确
			Process process = new Process();
			process.StartInfo.FileName = "powershell.exe";
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.Arguments = "Get-AppxPackage -AllUsers | Select-Object IsFramework,PackageFamilyName";
			//process.StartInfo.Arguments = "Get-AppxPackage | Select-Object IsFramework,PackageFamilyName";
			process.Start();

			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			// 分析输出以获取 UWP 应用信息
			string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in lines.Skip(3)) // 跳过前三行，标题分隔线
			{
				string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length >= 2)
				{
					string packageFullName = parts[1];
					//if ("True".Equals(parts[0])) continue; // 筛选掉框架应用
					if (uwpApplicationsList.ContainsKey(packageFullName))
					{
						//Console.WriteLine(tempList[packageFullName].Name);
						resultList.Add(uwpApplicationsList[packageFullName]);
					}
				}
			}
			uwpApplicationsList.Clear();
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

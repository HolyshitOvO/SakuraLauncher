using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HakeQuick.Helpers
{
    public static class Tools
    {
		/// <summary>
		/// 获取运行程序所在文件夹位置
		/// </summary>
		public static string GetApplicationFolderPath()
		{
			// 获取当前执行的程序集
			Assembly assembly = Assembly.GetEntryAssembly();
			// 获取程序集的位置
			string assemblyLocation = assembly.Location;
			// 获取程序所在路径
			string applicationPath = Path.GetDirectoryName(assemblyLocation);
			return applicationPath;
		}

		/// <summary>
		/// 获取运行程序所在文件夹位置
		/// </summary>
		public static string GetApplicationExePath()
		{
			// 获取当前执行的程序集
			Assembly assembly = Assembly.GetEntryAssembly();
			// 获取程序的位置
			return assembly.Location;
		}
	}
}

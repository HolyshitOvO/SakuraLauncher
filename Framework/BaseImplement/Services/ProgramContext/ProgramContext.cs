using CandyLauncher.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;

namespace CandyLauncher.Implementation.Services.ProgramContext
{
	internal sealed class ProgramContext : IProgramContext
	{

		private static class Win32
		{
			[DllImport("user32.dll")]
			public static extern int GetWindowRect(IntPtr hwnd, out RECT lpRect);
			[DllImport("user32.dll")]
			public static extern IntPtr GetForegroundWindow();
			[DllImport("user32.dll")]
			public static extern bool SetForegroundWindow(IntPtr ptr);
			[DllImport("User32.dll", CharSet = CharSet.Auto)]
			public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
			[DllImport("user32.dll")]
			public static extern IntPtr GetDesktopWindow();
			public const int GWL_STYLE = -16;
			public const uint WS_BORDER = 0x00800000;
			public const uint WS_CAPTION = 0x00C00000;

			[DllImport("user32.dll", SetLastError = true)]
			public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

		}


		public Process CurrentProcess { get; }
		public IntPtr WindowHandle { get; }
		public IntPtr DesktopHandle { get; }
		public int ThreadId { get; }
		public int ProcessId { get; }
		public bool IsDesktop { get { return WindowHandle == DesktopHandle; } }
		public RECT WindowPosition { get; }
		public FullscreenMode WindowScreenMode { get { return GetCurrentWindowMode(Win32.GetForegroundWindow()); } }

		public ProgramContext()
		{
			int pid;
			WindowHandle = Win32.GetForegroundWindow();
			ThreadId = Win32.GetWindowThreadProcessId(WindowHandle, out pid);
			ProcessId = pid;
			CurrentProcess = Process.GetProcessById(pid);
			DesktopHandle = Win32.GetDesktopWindow();
		}

		/// <summary>
		/// 获取目标窗口的全屏窗口模式
		/// </summary>
		/// <param name="WindowHandle">目标窗口</param>
		/// <returns></returns>
		public static FullscreenMode GetCurrentWindowMode(IntPtr WindowHandle)
		{
			// 获取目标窗口所在的屏幕
			Screen currentScreen = Screen.FromHandle(WindowHandle);
			//int screenWidth = currentScreen.Bounds.Width;
			//int screenHeight = currentScreen.Bounds.Height;
			Win32.GetWindowRect(WindowHandle, out RECT rect);

			// 检查窗口是否最大化并无边框
			uint style = Win32.GetWindowLong(WindowHandle, Win32.GWL_STYLE);

			if ((style & Win32.WS_BORDER) == 0 &&
				rect.Left == currentScreen.Bounds.Left &&
				rect.Top == currentScreen.Bounds.Top &&
				rect.Right == currentScreen.Bounds.Right &&
				rect.Bottom == currentScreen.Bounds.Bottom)
			{
				return FullscreenMode.Borderless;
			}

			// 检查窗口是否占满整个屏幕并隐藏任务栏
			if (rect.Left == currentScreen.Bounds.Left &&
				rect.Top == currentScreen.Bounds.Top &&
				rect.Right == currentScreen.Bounds.Right &&
				rect.Bottom == currentScreen.Bounds.Bottom &&
				(style & Win32.WS_CAPTION) == 0)
			{
				return FullscreenMode.Fullscreen;
			}

			// 默认是窗口模式
			return FullscreenMode.Windowed;
		}

	}
}

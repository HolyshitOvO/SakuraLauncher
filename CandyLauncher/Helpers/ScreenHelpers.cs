using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CandyLauncher.Helpers
{
	public enum DpiType
	{
		Effective = 0,
		Angular = 1,
		Raw = 2,
	}
	public static class ScreenHelpers
	{
		// Delegate for GetDpiForMonitor
		private delegate int GetDpiForMonitorDelegate(IntPtr hmonitor, DpiType dpiType, out uint dpiX, out uint dpiY);
		private static GetDpiForMonitorDelegate GetDpiForMonitorFunc;

		static ScreenHelpers()
		{
			try
			{
				// Try to load Shcore.dll and GetDpiForMonitor function
				IntPtr hModule = LoadLibrary("Shcore.dll");
				if (hModule != IntPtr.Zero)
				{
					IntPtr procAddress = GetProcAddress(hModule, "GetDpiForMonitor");
					if (procAddress != IntPtr.Zero)
					{
						GetDpiForMonitorFunc = Marshal.GetDelegateForFunctionPointer<GetDpiForMonitorDelegate>(procAddress);
					}
				}
			}
			catch
			{
				// Ignore errors, fallback to default methods
				GetDpiForMonitorFunc = null;
			}
		}

		public static List<uint> GetScreenDpis(IEnumerable<Screen> screens)
		{
			List<uint> dpis = new List<uint>();
			foreach (Screen screen in screens)
			{
				GetDpi(screen, DpiType.Effective, out uint dpix, out uint dpiy);
				dpis.Add(dpix);
			}
			return dpis;
		}

		public static void GetDpi(this Screen screen, DpiType dpiType, out uint dpiX, out uint dpiY)
		{
			// If Shcore.dll is available, use GetDpiForMonitor
			if (GetDpiForMonitorFunc != null)
			{
				var pnt = new Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
				var mon = MonitorFromPoint(pnt, 2 /*MONITOR_DEFAULTTONEAREST*/);
				GetDpiForMonitorFunc(mon, dpiType, out dpiX, out dpiY);
			}
			else
			{
				// Fallback for Windows 7: Use Graphics to estimate DPI
				using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
				{
					dpiX = (uint)g.DpiX;
					dpiY = (uint)g.DpiY;
				}
			}
		}

		// https://msdn.microsoft.com/en-us/library/windows/desktop/dd145062(v=vs.85).aspx
		[DllImport("User32.dll")]
		private static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

		// LoadLibrary to dynamically load Shcore.dll
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		// GetProcAddress to dynamically load GetDpiForMonitor
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
	}


	public static class ScreenHelpers2
	{
		public static List<uint> GetScreenDpis(IEnumerable<Screen> screens)
		{
			List<uint> dpis = new List<uint>();
			foreach (Screen screen in screens)
			{
				GetDpi2(screen, DpiType.Effective, out uint dpix, out uint dpiy);
				dpis.Add(dpix);
			}
			return dpis;
		}

		public static void GetDpi2(this Screen screen, DpiType dpiType, out uint dpiX, out uint dpiY)
		{
			var pnt = new System.Drawing.Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
			var mon = MonitorFromPoint(pnt, 2/*MONITOR_DEFAULTTONEAREST*/);
			GetDpiForMonitor(mon, dpiType, out dpiX, out dpiY);
		}

		//https://msdn.microsoft.com/en-us/library/windows/desktop/dd145062(v=vs.85).aspx
		[DllImport("User32.dll")]
		private static extern IntPtr MonitorFromPoint([In] System.Drawing.Point pt, [In] uint dwFlags);

		//https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510(v=vs.85).aspx
		[DllImport("Shcore.dll")]
		private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);
	}
}

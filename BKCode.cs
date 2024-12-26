//FileMenuUtil2.ShowShellContextMenur(action.Subtitle);
using static HakeQuick.Helpers.FileMenuUtil.API;

ContextMenuWrapper cmw = new ContextMenuWrapper();
cmw.OnQueryMenuItems += (QueryMenuItemsEventHandler)delegate (object s, QueryMenuItemsEventArgs args)
{
				args.ExtraMenuItems = new string[] { "Edit Dock Entry", "Remove Dock Entry", "---" };

				args.GrayedItems = new string[] { "delete", "rename", "cut", "copy" };
				args.HiddenItems = new string[] { "link" };
				args.DefaultItem = 1;
};
cmw.OnAfterPopup += (AfterPopupEventHandler)delegate (object s, AfterPopupEventArgs args)
{
				//Messenger.Default.Send<ShellContextMenuMessage>(ShellContextMenuMessage.Closed());
};

//Messenger.Default.Send<ShellContextMenuMessage>(ShellContextMenuMessage.Opened());
Debug.WriteLine(File.Exists(action.Subtitle) + "");
try
{
				//FileSystemInfoEx[] files = new[] { FileInfoEx.FromString(@action.Subtitle) };
				FileSystemInfoEx[] files = new[] { FileInfoEx.FromString(@"c:\windows\notepad.exe") };
				//int[] position = Win32Mouse.GetMousePosition();
				string command = cmw.Popup(files, new System.Drawing.Point(p.X, p.Y));

				// Handle the click on the 'ExtraMenuItems'.
				switch (command)
				{
					case "Edit Dock Entry":
						//Messenger.Default.Send<ApplicationMessage>(ApplicationMessage.Edit(application));
						break;
					case "Remove Dock Entry":
						//Messenger.Default.Send<ApplicationMessage>(ApplicationMessage.Remove(application));
						break;
				}
				e.Handled = true; // Don't open the normal context menu.
}
catch (Exception ex)
{
				Debug.Print("Problem displaying shell context menu: {0}", ex);
}


			//ListViewItem.Focus();
			//System.Windows.Controls.ListView item = sender as System.Windows.Controls.ListView;
			//if (item != null && item.SelectedItem != null)
			//{
			//	//MessageBox.Show(item.SelectedIndex.ToString());
			//}
			//e.Handled = true;


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using System.Windows.Forms;
using System.Windows;
using Shell32;
using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Runtime.InteropServices.ComTypes;
//using SharpShell.Interop;

namespace HakeQuick.Helpers
{

	//定义IShellFolder接口
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("000214E6-0000-0000-C000-000000000046")]
	public interface IShellFolder
	{
		void ParseDisplayName(IntPtr hwnd, IBindCtx pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, out uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
		//void EnumObjects(IntPtr hwnd, uint grfFlags, out IEnumIDList ppenumIDList);
		void BindToObject(IntPtr pidl, IBindCtx pbc, [In] ref Guid riid, out IntPtr ppv);
		void BindToStorage(IntPtr pidl, IBindCtx pbc, [In] ref Guid riid, out IntPtr ppv);
		void CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);
		void CreateViewObject(IntPtr hwndOwner, [In] ref Guid riid, out IntPtr ppv);
		void GetAttributesOf(uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref uint rgfInOut);
		void GetUIObjectOf(IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, [In] ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);
		void GetDisplayNameOf(IntPtr pidl, uint uFlags, out IntPtr pName);
		void SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
	}

	//定义IContextMenu接口
	//[ComImport]
	//[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	//[Guid("000214e4-0000-0000-c000-000000000046")]
	//public interface IContextMenu
	//{
	//	[PreserveSig]
	//	int QueryContextMenu(IntPtr hMenu, uint iMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
	//	void InvokeCommand(ref ShellAPI.CMINVOKECOMMANDINFOEX info);
	//	void GetCommandString(uint idcmd, uint uflags, uint reserved, [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring, int cch);
	//}

	//定义CMINVOKECOMMANDINFOEX结构体
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CMINVOKECOMMANDINFOEX
	{
		public int cbSize;
		public uint fMask;
		public IntPtr hwnd;
		public IntPtr lpVerb;
		[MarshalAs(UnmanagedType.LPStr)]
		public string lpParameters;
		[MarshalAs(UnmanagedType.LPStr)]
		public string lpDirectory;
		public int nShow;
		public uint dwHotKey;
		public IntPtr hIcon;
		[MarshalAs(UnmanagedType.LPStr)]
		public string lpTitle;
		public IntPtr lpVerbW;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpParametersW;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpDirectoryW;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpTitleW;
		public Point ptInvoke;
	}

	//定义常量
	public static class ShellAPI
	{
		public const int MAX_PATH = 260;
		public const uint SEE_MASK_INVOKEIDLIST = 12;
		public const uint SW_SHOWNORMAL = 1;
		public const uint CMF_DEFAULTONLY = 1;
		public const uint GCS_VERBW = 4;
		public const int S_OK = 0;
		public static Guid IID_IContextMenu = new Guid("000214e4-0000-0000-c000-000000000046");
		public static Guid IID_IShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");
	}

	//定义辅助方法
	public static class ShellHelper
	{
		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		public static extern int SHGetDesktopFolder(out IShellFolder ppshf);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		public static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		public static extern int SHBindToParent(IntPtr pidl, [In] ref Guid riid, out IntPtr ppv, out IntPtr ppidlLast);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern IntPtr GetMenu(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);

		[DllImport("user32.dll")]
		public static extern int GetMenuItemCount(IntPtr hMenu);

		[DllImport("user32.dll")]
		public static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

		[DllImport("user32.dll")]
		public static extern bool DestroyMenu(IntPtr hMenu);
	}

	public class FileMenuUtil2
	{




		//定义主方法
		public static void ShowShellContextMenur(string fileName)
		{
			//获取文件名
			//string fileName = args[0];
			//获取桌面IShellFolder
			IShellFolder desktopFolder;
			ShellHelper.SHGetDesktopFolder(out desktopFolder);
			//解析文件名到PIDL
			IntPtr filePidl;
			uint attr;
			ShellHelper.SHParseDisplayName(fileName, IntPtr.Zero, out filePidl, 0, out attr);
			//绑定到父目录的IShellFolder
			//IntPtr parentPidl;
			//IShellFolder parentFolder;
			//ShellHelper.SHBindToParent(filePidl, ref ShellAPI.IID_IShellFolder, out parentFolder, out parentPidl);

			IntPtr parentPidl;
			IntPtr parentFolderPtr; //修改为IntPtr
			ShellHelper.SHBindToParent(filePidl, ref ShellAPI.IID_IShellFolder, out parentFolderPtr, out parentPidl); //修改为out IntPtr
			IShellFolder parentFolder = (IShellFolder)Marshal.GetTypedObjectForIUnknown(parentFolderPtr, typeof(IShellFolder)); //使用Marshal转换为IShellFolder
																																//获取IContextMenu接口
			IntPtr contextMenuPtr;
			//IContextMenu contextMenu;
			parentFolder.GetUIObjectOf(IntPtr.Zero, 1, new IntPtr[] { parentPidl }, ref ShellAPI.IID_IContextMenu, IntPtr.Zero, out contextMenuPtr);
			//contextMenu = (IContextMenu)Marshal.GetTypedObjectForIUnknown(contextMenuPtr, typeof(IContextMenu));
			//获取菜单
			IntPtr menu = ShellHelper.GetMenu(ShellHelper.GetForegroundWindow());
			IntPtr subMenu = ShellHelper.GetSubMenu(menu, 0);
			int count = ShellHelper.GetMenuItemCount(subMenu);
			//contextMenu.QueryContextMenu(subMenu, (uint)count, 0, 0x7FFF, (CMF)ShellAPI.CMF_DEFAULTONLY);
			//弹出菜单
			int cmd = ShellHelper.TrackPopupMenuEx(subMenu, 0x100, 100, 100, ShellHelper.GetForegroundWindow(), IntPtr.Zero);
			//根据返回值调用命令
			if (cmd > 0)
			{
				StringBuilder verb = new StringBuilder(ShellAPI.MAX_PATH);
				//contextMenu.GetCommandString((int)(uint)cmd, (GCS)ShellAPI.GCS_VERBW, 0, verb, verb.Capacity);
				CMINVOKECOMMANDINFOEX info = new CMINVOKECOMMANDINFOEX();
				info.cbSize = Marshal.SizeOf(info);
				info.fMask = ShellAPI.SEE_MASK_INVOKEIDLIST;
				info.hwnd = ShellHelper.GetForegroundWindow();
				info.lpVerb = Marshal.StringToHGlobalAnsi(verb.ToString());
				info.lpVerbW = Marshal.StringToHGlobalUni(verb.ToString());
				info.nShow = (int)ShellAPI.SW_SHOWNORMAL;
				//info.lpDirectoryW = Marshal.StringToHGlobalUni(Path.GetDirectoryName(fileName));
				//contextMenu.InvokeCommand(ref info);
			}
			//释放资源
			ShellHelper.DestroyMenu(menu);
			//Marshal.ReleaseComObject(contextMenu);
			Marshal.ReleaseComObject(parentFolder);
			Marshal.ReleaseComObject(desktopFolder);
			Marshal.FreeCoTaskMem(filePidl);
			Marshal.FreeCoTaskMem(parentPidl);
		}
	}
}


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using System.Windows.Forms;
using System.Windows;
using Shell32;
using System.IO;
using System.Runtime.InteropServices;
namespace HakeQuick.Helpers
{

	public class FileMenuUtil
	{
		// 定义 SHELLEXECUTEINFO 结构体
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SHELLEXECUTEINFO
		{
			public int cbSize;
			public uint fMask;
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpVerb;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpFile;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpParameters;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpDirectory;
			public int nShow;
			public IntPtr hInstApp;
			public IntPtr lpIDList;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpClass;
			public IntPtr hkeyClass;
			public uint dwHotKey;
			public IntPtr hIcon;
			public IntPtr hProcess;
		}

		// 导入 ShellExecuteEx 函数
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

		public static void ShowFileContentMenu(string path)
		{
			// 创建一个 SHELLEXECUTEINFO 实例
			SHELLEXECUTEINFO sei = new SHELLEXECUTEINFO();
			sei.cbSize = Marshal.SizeOf(sei);
			sei.fMask = 0x00000040; // SEE_MASK_INVOKEIDLIST
			sei.hwnd = IntPtr.Zero;
			sei.lpVerb = "properties"; // 操作类型为右键菜单
									   //sei.lpVerb = "properties"; // 操作类型为右键菜单
			sei.lpFile = @path; // 文件路径
			sei.nShow = 1; // SW_SHOWNORMAL
			sei.hInstApp = IntPtr.Zero;

			// 调用 ShellExecuteEx 函数
			bool result = ShellExecuteEx(ref sei);
			if (result)
			{
				Debug.WriteLine("成功显示右键菜单");
			}
			else
			{
				Debug.WriteLine("显示右键菜单失败");
			}
		}






		//定义一些常量和结构体
		public static class API
		{
			public const uint CMD_FIRST = 1;
			public const uint CMD_LAST = 30000;

			public const uint TPM_RETURNCMD = 0x0100;

			public enum CMF : uint
			{
				NORMAL = 0x00000000,
				DEFAULTONLY = 0x00000001,
				VERBSONLY = 0x00000002,
				EXPLORE = 0x00000004,
				NOVERBS = 0x00000008,
				CANRENAME = 0x00000010,
				NODEFAULT = 0x00000020,
				INCLUDESTATIC = 0x00000040,
				EXTENDEDVERBS = 0x00000100,
				RESERVED = 0xffff0000
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct POINT
			{
				public int x;
				public int y;

				public POINT(int x, int y)
				{
					this.x = x;
					this.y = y;
				}
			}

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			public struct CMINVOKECOMMANDINFOEX
			{
				public int cbSize;
				public uint fMask;
				public IntPtr hwnd;
				public IntPtr lpVerb;
				[MarshalAs(UnmanagedType.LPStr)]
				public string lpParameters;
				[MarshalAs(UnmanagedType.LPStr)]
				public string lpDirectory;
				public int nShow;
				public uint dwHotKey;
				public IntPtr hIcon;
				[MarshalAs(UnmanagedType.LPStr)]
				public string lpTitle;
				public IntPtr lpVerbW;
				[MarshalAs(UnmanagedType.LPWStr)]
				public string lpParametersW;
				[MarshalAs(UnmanagedType.LPWStr)]
				public string lpDirectoryW;
				[MarshalAs(UnmanagedType.LPWStr)]
				public string lpTitleW;
				public POINT ptInvoke;
			}

			[DllImport("user32.dll")]
			public static extern IntPtr CreatePopupMenu();

			[DllImport("user32.dll")]
			public static extern bool DestroyMenu(IntPtr hMenu);

			[DllImport("user32.dll")]
			public static extern uint TrackPopupMenuEx(IntPtr hMenu, uint uFlags, int x, int y, IntPtr hWnd, IntPtr tpmParams);
		}

		//定义一些接口
		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("000214E6-0000-0000-C000-000000000046")]
		public interface IShellFolder
		{
			void ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, out uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
			void EnumObjects(IntPtr hwnd, uint grfFlags, out IEnumIDList ppenumIDList);
			void BindToObject(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, out IShellFolder ppv);
			void BindToStorage(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, out IntPtr ppv);
			void CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);
			void CreateViewObject(IntPtr hwndOwner, [In] ref Guid riid, out IntPtr ppv);
			void GetAttributesOf(uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref uint rgfInOut);
			void GetUIObjectOf(IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, [In] ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);
			void GetDisplayNameOf(IntPtr pidl, uint uFlags, out IntPtr pName);
			void SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("000214F4-0000-0000-C000-000000000046")]
		public interface IContextMenu
		{
			[PreserveSig]
			int QueryContextMenu(IntPtr hMenu, uint iMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
			void InvokeCommand(ref API.CMINVOKECOMMANDINFOEX info);
			void GetCommandString(uint idCmd, uint uFlags, uint pwReserved, [MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName, uint cchMax);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("000214F2-0000-0000-C000-000000000046")]
		public interface IEnumIDList
		{
			[PreserveSig]
			int Next(uint celt, out IntPtr rgelt, out uint pceltFetched);
			void Skip(uint celt);
			void Reset();
			void Clone(out IEnumIDList ppenum);
		}


		[DllImport("shell32.dll")]
		static extern int SHGetDesktopFolder(out IShellFolder ppshf);


		//定义一个类，用来封装 IShellFolder 和 IContextMenu 的操作
		public class ShellHelper
		{
			//获取桌面的 IShellFolder 接口
			public static IShellFolder GetDesktopFolder()
			{
				IShellFolder folder;
				Guid iidShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");
				SHGetDesktopFolder(out folder);
				return folder;
			}

			//根据路径获取 IShellFolder 接口
			public static IShellFolder GetShellFolder(string path)
			{
				IShellFolder folder = GetDesktopFolder();
				uint eaten;
				uint attributes = 0;
				IntPtr pidl;
				folder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, path, out eaten, out pidl, ref attributes);
				Guid iidShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");
				folder.BindToObject(pidl, IntPtr.Zero, ref iidShellFolder, out folder);
				return folder;
			}

			//根据路径获取 IContextMenu 接口
			public static IContextMenu GetContextMenu(string path)
			{
				IShellFolder folder = GetShellFolder(Path.GetDirectoryName(path));
				uint eaten;
				uint attributes = 0;
				IntPtr pidl;
				folder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, path, out eaten, out pidl, ref attributes);
				IntPtr[] pidls = new IntPtr[] { pidl };
				IntPtr contextMenuPtr;
				Guid iidContextMenu = new Guid("000214F4-0000-0000-C000-000000000046");
				folder.GetUIObjectOf(IntPtr.Zero, 1, pidls, ref iidContextMenu, IntPtr.Zero, out contextMenuPtr);
				IContextMenu contextMenu = (IContextMenu)Marshal.GetObjectForIUnknown(contextMenuPtr);
				return contextMenu;
			}

			//弹出上下文菜单
			public static void ShowContextMenu(string path, Point location)
			{
				IContextMenu contextMenu = GetContextMenu(path);
				IntPtr menu = API.CreatePopupMenu();
				contextMenu.QueryContextMenu(menu, 0, API.CMD_FIRST, API.CMD_LAST, (uint)(API.CMF.NORMAL | API.CMF.EXPLORE));
				uint cmd = API.TrackPopupMenuEx(menu, API.TPM_RETURNCMD, (int)location.X, (int)location.Y, Form.ActiveForm.Handle, IntPtr.Zero);
				if (cmd >= API.CMD_FIRST)
				{
					API.CMINVOKECOMMANDINFOEX invoke = new API.CMINVOKECOMMANDINFOEX();
					invoke.cbSize = Marshal.SizeOf(typeof(API.CMINVOKECOMMANDINFOEX));
					invoke.lpVerb = (IntPtr)(cmd - 1);
					invoke.lpDirectory = string.Empty;
					invoke.fMask = 0;
					invoke.ptInvoke = new API.POINT((int)location.X, (int)location.Y);
					invoke.nShow = 1;
					contextMenu.InvokeCommand(ref invoke);
				}
				API.DestroyMenu(menu);
			}
		}
	}
}

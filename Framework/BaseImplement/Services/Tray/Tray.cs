using HakeQuick.Helpers;
using HakeQuick.Implementation.Services.TerminationNotifier;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HakeQuick.Implementation.Services.Tray
{
	public enum TRAY_DOSOMETHING
	{
		OPEN_MYAPP_FOLDER,
		EXIT_APP,
		REFRESH_DATA,
		EDIT_RUNITEM_PREFFILE,
		EDIT_PREFFILE,
		RESTART_APP,
		GO_APP_GITHUB_HOME,
        OPEN_MAIN_PANEL,
	}

	internal sealed class Tray : ITray, IDisposable
	{
		private ITerminationNotifier terminationNotifier;
		private NotifyIcon tray = null;


		public Tray(ITerminationNotifier terminationNotifier)
		{
			this.terminationNotifier = terminationNotifier;

			tray = new NotifyIcon();
			Assembly assembly = Assembly.GetEntryAssembly();
			Stream iconStream = assembly.LoadStream("HakeQuick.tray.ico");
			tray.Icon = new Icon(iconStream);
			iconStream.Close();
			iconStream.Dispose();
			tray.Visible = true;
			// 右键托盘，菜单项
			MenuItem[] menuitems = new MenuItem[]
			{
			new MenuItem("打开主面板", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.OPEN_MAIN_PANEL)),
			new MenuItem("打开本程序路径", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.OPEN_MYAPP_FOLDER)),
			new MenuItem("编辑配置文件", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.EDIT_PREFFILE)),
			new MenuItem("编辑运行配置文件", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.EDIT_RUNITEM_PREFFILE)),
			new MenuItem("-"),
			new MenuItem("前往软件项目主页", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.GO_APP_GITHUB_HOME)),
			new MenuItem("-"),
			new MenuItem("重启软件", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.RESTART_APP)),
			new MenuItem("退出", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.EXIT_APP)),
			};
			tray.ContextMenu = new ContextMenu(menuitems);
		}

		private bool disposed = false;
		~Tray()
		{
			if (!disposed)
				Dispose();
		}
		public void Dispose()
		{
			if (disposed)
				return;

			tray.Visible = false;
			tray.Dispose();
			disposed = true;
		}

		public void SendNotification(int timeout, string title, string content, ToolTipIcon icon)
		{
			tray.ShowBalloonTip(timeout, title, content, icon);
		}
	}
}

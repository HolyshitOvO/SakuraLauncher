using HakeQuick.Helpers;
using HakeQuick.Implementation.Services.TerminationNotifier;
using System;
using System.Collections.Generic;
using System.Drawing;
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
			MenuItem openAppExeFolderMenu = new MenuItem("打开本程序路径", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.OPEN_MYAPP_FOLDER));
			MenuItem openRunPrefFileMenu = new MenuItem("编辑运行配置文件", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.EDIT_RUNITEM_PREFFILE));
			MenuItem openPrefFileMenu = new MenuItem("编辑配置文件", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.EDIT_PREFFILE));
			MenuItem goAppGithubMenu = new MenuItem("前往软件项目主页", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.GO_APP_GITHUB_HOME));
			MenuItem restartAppMenu = new MenuItem("重启软件", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.RESTART_APP));
			MenuItem closeMenu = new MenuItem("退出", (sender, e) => this.terminationNotifier.NotifyTerminate(TRAY_DOSOMETHING.EXIT_APP));

			// todo: 添加分隔线
			MenuItem[] menuitems = new MenuItem[] { openAppExeFolderMenu, openPrefFileMenu, openRunPrefFileMenu, goAppGithubMenu, restartAppMenu, closeMenu };
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

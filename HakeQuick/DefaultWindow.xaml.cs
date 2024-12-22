using HakeQuick.Implementation.Services.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HakeQuick.Abstraction.Action;
using HakeQuick.Abstraction.Services;
using System.Collections.ObjectModel;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using HakeQuick.Helpers;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using ContextMenuUtil;
using System.Threading;
using System.Reflection.Emit;

namespace HakeQuick
{
	public partial class DefaultWindow : Window, IQuickWindow
	{
		public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		/// <summary>
		/// 输入框文本
		/// </summary>
		public string RawInput
		{
			get { return textbox_input.Text; }
			set
			{
				textbox_input.Text = value;
			}
		}
		/// <summary>
		/// 订阅事件，当文本内容更改时
		/// </summary>
		public event EventHandler<TextUpdatedEventArgs> TextChanged;
		/// <summary>
		/// 订阅事件，窗口输入 Enter 按键
		/// </summary>
		public event EventHandler<ExecutionRequestedEventArgs> ExecutionRequested;
		/// <summary>
		/// 订阅事件，窗口输入 Ctrl + O 按键
		/// </summary>
		public event EventHandler<ExecutionFolderRequestedEventArgs> ExecutionFolderRequested;
		/// <summary>
		/// 订阅事件，窗口可见性改变
		/// </summary>
		public event EventHandler VisibleChanged;


		private ObservableCollection<ActionBase> m_actions = null;
		private IntPtr hwnd;

		public DefaultWindow()
		{
			InitializeComponent();
			Loaded += OnLoaded;
		}



		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			hwnd = new WindowInteropHelper(this).Handle;
			GlassHelper.EnableAero(hwnd);

			textbox_input.TextChanged += OnTextChanged;
			IsVisibleChanged += OnIsVisibleChanged;
			//list_actions.PreviewMouseLeftButtonDown += OnListPreviewLeftMouseButtonDown;
			//list_actions.PreviewMouseRightButtonDown += OnListPreviewRightMouseButtonDown;
			Deactivated += OnDeactived;
			PreviewKeyDown += OnPreviewKeyDown;
			ElementHost.EnableModelessKeyboardInterop(this);
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Handled) return;
            if (m_actions == null || m_actions.Count <= 0 || IsVisible == false) return;

			if (e.Key == Key.Down)
			{
				MoveToNextAction();
				e.Handled = true;
			}
			else if (e.Key == Key.Up)
			{
				MoveToPreviousAction();
				e.Handled = true;
			}
			else if (e.Key == Key.Enter)
			{
				ActionBase action = list_actions.SelectedItem as ActionBase;
				if (!action.IsExecutable) { e.Handled = true; return; }
				ExecutionRequested?.Invoke(this, new ExecutionRequestedEventArgs(action));
				e.Handled = true;
			}
			// Ctrl+O，打开目标所在文件夹
			else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
			{
				ActionBase action = list_actions.SelectedItem as ActionBase;
				if (!action.IsExecutable) { e.Handled = true; return; }
				ExecutionFolderRequested?.Invoke(this, new ExecutionFolderRequestedEventArgs(action, USER_DOSOMETHING.OPEN_FOLDER));
				e.Handled = true;
			}
			else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I)
			{
				ActionBase action = list_actions.SelectedItem as ActionBase;
				if (!action.IsExecutable) { e.Handled = true; return; }
				ExecutionFolderRequested?.Invoke(this, new ExecutionFolderRequestedEventArgs(action, USER_DOSOMETHING.OPEN_GOAL_FOLDER));
				e.Handled = true;
			}

		}

		private void OnDeactived(object sender, EventArgs e)
		{
			HideWindow();
		}

		private void OnListPreviewLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		/// <summary>
		/// 获取右键点击的item相关方法
		/// </summary>
		static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
		{
			while (source != null && source.GetType() != typeof(T))
				source = VisualTreeHelper.GetParent(source);
			return source;
		}

		/// <summary>
		/// 获取当前鼠标位置，x，y
		/// </summary>
		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out POINT lpPoint);

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;
		}

		private bool isNeedShowContextMenu = false;
		//private string needShowContextMenuFilePath = @"c:\windows\notepad.exe";
		private string needShowContextMenuFilePath = @"C:\Users\Default\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Windows PowerShell\Windows PowerShell.lnk";

		/// <summary>
		/// 当右键点击 listview item
		/// </summary>
		private void OnListRightMouseButtonUp(object sender, MouseButtonEventArgs e)
		{
			// 获取右键点击的item 数据 DataContext，相关方法
			System.Windows.Controls.ListViewItem ListViewItem = VisualUpwardSearch<System.Windows.Controls.ListViewItem>(e.OriginalSource as DependencyObject) as System.Windows.Controls.ListViewItem;
			if (ListViewItem != null)
			{
				ActionBase action = ListViewItem.DataContext as ActionBase;
				Debug.WriteLine(action.Subtitle);
				if (action.Subtitle.Equals("UWP APP"))
				{

				}
				else
				{
					GetCursorPos(out POINT p);
					// 弹出该文件的系统右键菜单
					try
					{
						ShellContextMenu scm = new ShellContextMenu();
						FileInfo[] files2 = new FileInfo[1];
						files2[0] = new FileInfo(action.Subtitle);
						scm.ShowContextMenu(files2, new System.Drawing.Point(p.X, p.Y));
					}
					catch (Exception ex)
					{
						Debug.Print("显示菜单出错: {0}", ex);
					}
				}
				e.Handled = true;
			}
			return;
		}

		private void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			if (e.Handled)
				return;

			TextChanged?.Invoke(this, new TextUpdatedEventArgs(textbox_input.Text));
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			VisibleChanged?.Invoke(this, null);
		}

		public void ClearInput()
		{
			textbox_input.Text = "";
			TextChanged?.Invoke(this, new TextUpdatedEventArgs(textbox_input.Text));
		}

		public void HideWindow()
		{
			if (IsVisible == false)
				return;
			this.Hide();
		}

		public void SetActions(ObservableCollection<ActionBase> actions)
		{
			list_actions.ItemsSource = actions;
			m_actions = actions;
		}

		/// <summary>
		/// 弹出主窗口，并调整布局、位置
		/// </summary>
		/// <param name="context"></param>
		public void ShowWindow(IProgramContext context)
		{
			if (IsVisible == true)
				return;
			// 获取所有屏幕信息
			//System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;
			//screens[0].GetDpi(DpiType.Effective, out uint dpiX, out uint dpiY);
			System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.PrimaryScreen;
			screen.GetDpi(DpiType.Effective, out uint dpiX, out uint dpiY);
			float dpiScaleX = dpiX / 96.0f;
			float dpiScaleY = dpiY / 96.0f;
			if (true)
			{
				// 固定屏幕中间
				Left = (screen.Bounds.Width / dpiScaleX / 2) - Width / 2;
				Top = (screen.Bounds.Height / dpiScaleY) * 0.22;
			}
			else
			{
				// 获取每个屏幕的DPI 值
				//List<uint> dpis = ScreenHelpers.GetScreenDpis(screen);
				// 传入对象的窗口位置信息
				RECT position = context.WindowPosition;
				//double ttop = position.Top + 50;
				// 根据窗口位置，再往下100
				double ttop = position.Top + 200;
				double windowWidth = ActualWidth;
				if (windowWidth <= 0)
					windowWidth = Width;
				// 计算缩放比例 
				//uint scale = dpis[0] / 96;
				//windowWidth *= scale;
				double halfwidthdiff = ((position.Right - position.Left) - windowWidth) / 2;
				double tleft = position.Left + halfwidthdiff;
				if (tleft < 0 && tleft + windowWidth > 0)
					tleft = -(windowWidth + 50);
				if (ttop < 200)
					ttop = 200;
				// 得到缩放后的位置
				//Left = tleft / scale;
				//Top = ttop / scale;
			}
			// 显示窗口
			Show();
			// 激活窗口
			Activate();
			// 使聚焦输入框
			textbox_input.Focus();
			// 置顶显示
			SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, 3);
			Activate();
		}

		private void MoveToNextAction()
		{
			int index = list_actions.SelectedIndex;
			if (index == -1)
			{
				for (int i = 0; i < m_actions.Count; i++)
				{
					if (m_actions[i].IsExecutable)
					{
						list_actions.SelectedIndex = i;
						list_actions.ScrollIntoView(list_actions.SelectedItem);
						break;
					}
				}
			}
			else
			{
				int oindex = index;
				do
				{
					index++;
					if (index < 0)
						index = m_actions.Count - 1;
					if (index >= m_actions.Count)
						index = 0;
					if (index == oindex)
						break;
				} while (!m_actions[index].IsExecutable);
				if (m_actions[index].IsExecutable)
				{
					list_actions.SelectedIndex = index;
					list_actions.ScrollIntoView(list_actions.SelectedItem);
				}
				else
					list_actions.SelectedIndex = -1;
			}
		}
		private void MoveToPreviousAction()
		{
			int index = list_actions.SelectedIndex;
			if (index == -1)
			{
				for (int i = m_actions.Count - 1; i >= 0; i--)
				{
					if (m_actions[i].IsExecutable)
					{
						list_actions.SelectedIndex = i;
						list_actions.ScrollIntoView(list_actions.SelectedItem);
						break;
					}
				}
			}
			else
			{
				int oindex = index;
				do
				{
					index--;
					if (index < 0)
						index = m_actions.Count - 1;
					if (index >= m_actions.Count)
						index = 0;
					if (index == oindex)
						break;
				} while (!m_actions[index].IsExecutable);
				if (m_actions[index].IsExecutable)
				{
					list_actions.SelectedIndex = index;
					list_actions.ScrollIntoView(list_actions.SelectedItem);
				}
				else
					list_actions.SelectedIndex = -1;
			}
		}

		public void OnActionUpdateCompleted()
		{
			list_actions.SelectedIndex = -1;
			MoveToNextAction();
		}


		public void SetKeyDown(System.Windows.Input.KeyEventHandler keyEventHandler)
		{
			this.KeyDown += keyEventHandler;
			//throw new NotImplementedException();
		}
	}
}

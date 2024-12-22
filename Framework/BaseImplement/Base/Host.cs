using Hake.Extension.DependencyInjection.Abstraction;
using HakeQuick.Abstraction.Action;
using HakeQuick.Abstraction.Base;
using HakeQuick.Abstraction.Services;
using HakeQuick.Implementation.Services.HotKey;
using HakeQuick.Implementation.Services.ProgramContext;
using HakeQuick.Implementation.Services.TerminationNotifier;
using HakeQuick.Implementation.Services.Tray;
using HakeQuick.Implementation.Services.Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace HakeQuick.Implementation.Base
{
    internal sealed class Host : IHost
    {
        private readonly IServiceProvider services = null;
        private readonly IServiceCollection pool = null;
        private readonly IAppBuilder appBuilder = null;
        private readonly ITerminationNotifier terminationNotifier = null;
        private readonly ITray tray = null;
        private readonly IHotKey hotkey = null;
        private readonly IQuickWindow window = null;
        private readonly ObservableCollection<ActionBase> actions;
        private Task waitTask;
        private readonly object locker = new object();
        private readonly AutoResetEvent mutex = new AutoResetEvent(true);
        private IInternalContext lastContext;


        public Host(IServiceProvider services, IServiceCollection pool, IAppBuilder appBuilder, ITerminationNotifier terminationNotifier, ITray tray, IHotKey hotkey, IQuickWindow window)
        {
            this.services = services;
            this.pool = pool;
            this.appBuilder = appBuilder;
            this.terminationNotifier = terminationNotifier;
            this.tray = tray;
            this.hotkey = hotkey;
            this.window = window;
            actions = new ObservableCollection<ActionBase>();
        }

        public void Run()
        {
            Application.ApplicationExit += OnApplicationExit;
            try
            {
                OnRun();

                // 右下角系统通知
                //tray.SendNotification(2000, "糖果启动器", "糖果启动器正在运行", ToolTipIcon.Info);
                terminationNotifier.TerminationNotified += OnTerminationNotified;
                hotkey.KeyPressed += OnHotKeyPressed;
                // 将热键绑定到应用程序
                hotkey.BindKey();
                Application.Run();

            }
            catch
            {
                OnExit();
            }
        }

        /// <summary>
        /// 右键托盘，菜单行为
        /// </summary>
        private void OnTerminationNotified(object sender, EventArgs e)
        {
            TRAY_DOSOMETHING wantToDo = (TRAY_DOSOMETHING)sender;
            switch (wantToDo)
            {
                case TRAY_DOSOMETHING.EXIT_APP:
                    OnExit();
                    break;
                case TRAY_DOSOMETHING.OPEN_MYAPP_FOLDER:
                    // 打开程序所在的文件夹
                    Process.Start("explorer.exe", "/select, \"" + Helpers.Tools.GetApplicationExePath() + "\"");
                    break;
                case TRAY_DOSOMETHING.EDIT_PREFFILE:
                    {
                        // 打开配置文件
                        string filePath = Path.Combine(Helpers.Tools.GetApplicationFolderPath(), "settings.json");
                        if (File.Exists(filePath))
                            Process.Start("\"" + filePath + "\"");
                        break;
                    }
                case TRAY_DOSOMETHING.EDIT_RUNITEM_PREFFILE:
                    {
                        // 打开配置文件
                        string filePath = Path.Combine(Helpers.Tools.GetApplicationFolderPath(), "runner.json");
                        if (File.Exists(filePath))
                            Process.Start("\"" + filePath + "\"");
                        break;
                    }
                case TRAY_DOSOMETHING.RESTART_APP:
                    {
                        // 重启软件
                        Process.Start("\"" + Helpers.Tools.GetApplicationExePath() + "\"");
                        OnExit();
                        break;
                    }
                case TRAY_DOSOMETHING.GO_APP_GITHUB_HOME:
                    Process.Start("\"" + "https://github.com/CandyTek/CandyLauncher" + "\"");
                    break;
                case TRAY_DOSOMETHING.OPEN_MAIN_PANEL:
                    OnHotKeyPressed(null, 0);
                    break;
                case TRAY_DOSOMETHING.REFRESH_DATA:
                    // TODO 刷新快捷方式数据
                    break;

            }
        }

        /// <summary>
        /// 模拟键盘输入
        /// </summary>
        /// <param name="bVk">要模拟的虚拟键码（Virtual Key Code）</param>
        /// <param name="bScan">硬件扫描码（Hardware Scan Code）</param>
        /// <param name="dwFlags">操作的标志位</param>
        /// <param name="dwExtraInfo">与事件相关的附加信息</param>
        //[DllImport("user32.dll", SetLastError = true)]
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        const int VK_F4 = 0x73; // F4键
        const int VK_F5 = 0x74; // F5键
        const int VK_C = 0x43; // C键
        const int VK_D = 0x44; // D键
        const int VK_V = 0x56; // V键
        const int VK_X = 0x58; // X键
        const int KEYEVENTF_KEYUP = 0x02; // 按键释放
        const int VK_TAB = 0x09; // Tab键
        const int VK_SHIFT = 0x10; // Shift键
        const int VK_CONTROL = 0x11; // Control键
        const int VK_MENU = 0x12; // Alt键
        const int VK_PAGEUP = 0x21; // Page Up键
        const int VK_PAGEDOWN = 0x22; // Page Down键
        const int VK_ESC = 0x1B; // ESC键
        const int VK_LWIN = 0x5B; // 左Windows键

        /// <summary>
        /// 全局热键事件（快捷键按下）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="hotkeyid"></param>
        private void OnHotKeyPressed(IHotKey sender, int hotkeyid)
        {
            // 模拟释放Alt键
            // TODO 这里还是有点问题，使用快捷键进行隐藏的时候，在下次启动打字仍然有些卡顿，esc倒不会
            keybd_event(VK_MENU, 0x45, KEYEVENTF_KEYUP, 0);
            if (window.IsVisible)
            {
                window.HideWindow();
                return;
            }
            pool.EnterScope();
            IProgramContext context = services.GetService<IProgramContext>();

            window.ShowWindow(context);
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            OnExit();
        }

        /// <summary>
        /// 初始化时来这运行一下
        /// </summary>
        private void OnRun()
        {
            window.SetActions(actions);
            // 注册事件
            window.VisibleChanged += OnWindowVisibleChanged;
            window.TextChanged += OnWindowTextChanged;
            window.ExecutionRequested += OnWindowExecutionRequested;
            window.ExecutionFolderRequested += OnWindowExecutionFolderRequested;
            window.SetKeyDown(new System.Windows.Input.KeyEventHandler(MyWindow_KeyDown));
            window.HideWindow();
        }
        /// <summary>
        /// 添加窗口键盘事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                // 处理 Esc 快捷键的逻辑代码
                window.HideWindow();
            }
            else if (e.Key == System.Windows.Input.Key.LeftAlt)
            {
                // TODO 这并没有用，这里是尝试解决快捷键关闭后，输入卡顿的问题
                e.Handled = true; // 阻止 Alt 键的默认行为
            }
        }

        /// <summary>
        /// 当窗口的可见状态发生变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowVisibleChanged(object sender, EventArgs e)
        {
            if (window.IsVisible)
            {
                window.TextChanged += OnWindowTextChanged;
                window.ExecutionRequested += OnWindowExecutionRequested;
                window.ExecutionFolderRequested += OnWindowExecutionFolderRequested;
                window.ClearInput();
            }
            else
            {
                window.TextChanged -= OnWindowTextChanged;
                window.ExecutionRequested -= OnWindowExecutionRequested;
                window.ExecutionFolderRequested -= OnWindowExecutionFolderRequested;
                pool.LeaveScope();
            }
        }
        /// <summary>
        /// 执行一些item的快捷键功能
        /// 打开item项所在文件夹位置，打开item项目标所在文件夹位置
        /// </summary>
        private void OnWindowExecutionFolderRequested(object sender, ExecutionFolderRequestedEventArgs e)
        {
            ActionBase action = e.Action;
            USER_DOSOMETHING wantToDo = e.WantToDo;

            try
            {
                if (lastContext == null)
                {
                    //ObjectFactory.InvokeMethod(action, "Invoke", services);
                }
                else
                {
                    object[] args = new object[lastContext.Command.UnnamedArguments.Count + 2];
                    args[0] = lastContext;
                    args[1] = lastContext.Command;
                    if (lastContext.Command.UnnamedArguments.Count > 0)
                        lastContext.Command.UnnamedArguments.CopyTo(args, 2);
                    if (lastContext.Command.UnnamedArguments.Count > 0)
                        lastContext.Command.UnnamedArguments.CopyTo(args, 1);
                    switch (wantToDo)
                    {
                        case USER_DOSOMETHING.OPEN_FOLDER:
                            {
                                ObjectFactory.InvokeMethod(action, "InvokeOpenFolder");
                                break;
                            }
                        case USER_DOSOMETHING.OPEN_GOAL_FOLDER:
                            {
                                ObjectFactory.InvokeMethod(action, "InvokeOpenGoalFolder");
                                break;
                            }
                    }
                }
                window.HideWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 开始执行item项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowExecutionRequested(object sender, ExecutionRequestedEventArgs e)
        {
            ActionBase action = e.Action;
            if (!action.IsExecutable) return;

            try
            {
                if (lastContext == null)
                    ObjectFactory.InvokeMethod(action, "Invoke", services);
                else
                {
                    object[] args = new object[lastContext.Command.UnnamedArguments.Count + 2];
                    args[0] = lastContext;
                    args[1] = lastContext.Command;
                    if (lastContext.Command.UnnamedArguments.Count > 0)
                        lastContext.Command.UnnamedArguments.CopyTo(args, 2);
                    if (lastContext.Command.UnnamedArguments.Count > 0)
                        lastContext.Command.UnnamedArguments.CopyTo(args, 1);
                    ObjectFactory.InvokeMethod(action, "Invoke", services, lastContext.Command.NamedArguments, args);
                }
                window.HideWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 当编辑框 文本更改时
        /// 涉及一系列的异步操作和线程间的同步控制
        /// 执行一个新的命令并等待执行完成，然后将结果通知到主线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowTextChanged(object sender, TextUpdatedEventArgs e)
        {
            SynchronizationContext syncContext = SynchronizationContext.Current;
            Task.Run(async () =>
            {
                // 等待互斥锁
                mutex.WaitOne();
                // 检查是否存在上一个上下文，任务是否完成
                if (lastContext != null && waitTask.IsCompleted == false)
                {
                    if (waitTask.IsCompleted == false)
                    {
                        lastContext.InternalCancellationProvider.Cancel();
                        waitTask.Wait();
                    }
                    // 释放上一个资源
                    lastContext.Dispose();
                }
                Command command = new Command(e.Value);
                IInternalContext context = new QuickContext(command);
                AppDelegate app = appBuilder.Build();
                await app(context);
                lastContext = context;
                syncContext.Send(s =>
                    waitTask = lastContext.WaitResults(actions).ContinueWith(tsk =>
                    {
                        if (tsk.Status == TaskStatus.RanToCompletion)
                            syncContext.Send(st => window.OnActionUpdateCompleted(), null);
                    }), null);
                // 释放互斥锁
                mutex.Set();
            });
        }

        private void OnExit()
        {
            Application.ApplicationExit -= OnApplicationExit;

            pool.Dispose();
            Application.ExitThread();
            Application.Exit();
        }
    }
}

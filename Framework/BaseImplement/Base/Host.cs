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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HakeQuick.Implementation.Base
{
    internal sealed class Host : IHost
    {
        private IServiceProvider services = null;
        private IServiceCollection pool = null;
        private IAppBuilder appBuilder = null;
        private ITerminationNotifier terminationNotifier = null;
        private ITray tray = null;
        private IHotKey hotkey = null;
        private IQuickWindow window = null;
        private ObservableCollection<ActionBase> actions;
        private Task waitTask;
        private object locker = new object();
        private AutoResetEvent mutex = new AutoResetEvent(true);
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
                tray.SendNotification(2000, "HakeQuick", "HakeQuick正在运行", ToolTipIcon.Info);
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

        private void OnTerminationNotified(object sender, EventArgs e)
        {
            OnExit();
        }

        /// <summary>
        /// 全局热键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="hotkeyid"></param>
        private void OnHotKeyPressed(IHotKey sender, int hotkeyid)
        {
            if (window.IsVisible)
            {
                window.HideWindow();
                return;
            }
            pool.EnterScope();
            IProgramContext context = services.GetService<IProgramContext>();
            window.ClearInput();
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
                window.ClearInput();
            }
            else
            {
                window.TextChanged -= OnWindowTextChanged;
                window.ExecutionRequested -= OnWindowExecutionRequested;
                pool.LeaveScope();
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

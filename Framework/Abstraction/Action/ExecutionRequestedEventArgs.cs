using System;

namespace HakeQuick.Abstraction.Action
{
    /// <summary>
    /// 订阅事件，窗口输入 Enter 按键
    /// </summary>
    public sealed class ExecutionRequestedEventArgs : EventArgs
    {
        public ActionBase Action { get; }

        public ExecutionRequestedEventArgs(ActionBase action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            Action = action;
        }
    }

    /// <summary>
    /// 订阅事件，窗口输入 Ctrl + O 按键
    /// </summary>
    public sealed class ExecutionFolderRequestedEventArgs : EventArgs
    {
        public ActionBase Action { get; }

        public ExecutionFolderRequestedEventArgs(ActionBase action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            Action = action;
        }
    }
}

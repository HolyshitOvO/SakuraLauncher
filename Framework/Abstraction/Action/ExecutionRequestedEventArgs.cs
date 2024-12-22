using System;

namespace CandyLauncher.Abstraction.Action
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
    /// 订阅事件，窗口输入其他功能按键
    /// </summary>
    public sealed class ExecutionFolderRequestedEventArgs : EventArgs
    {
        public ActionBase Action { get; }

        public USER_DOSOMETHING WantToDo { get; }

        public ExecutionFolderRequestedEventArgs(ActionBase action, USER_DOSOMETHING wantToDo)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            Action = action;
			WantToDo = wantToDo;
        }
    }

	public enum USER_DOSOMETHING
	{
		OPEN_FOLDER,
		OPEN_GOAL_FOLDER,
	}

}

using CandyLauncher.Abstraction.Action;
using CandyLauncher.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CandyLauncher.Implementation.Services.Window
{
    public class TextUpdatedEventArgs : EventArgs
    {
        public string Value { get; private set; }

        public TextUpdatedEventArgs(string value)
        {
            Value = value;
        }
    }
    public interface IQuickWindow
    {
        event EventHandler<TextUpdatedEventArgs> TextChanged;
        /// <summary>
        /// 确定按键
        /// </summary>
        event EventHandler<ExecutionRequestedEventArgs> ExecutionRequested;
        /// <summary>
        /// Ctrl + o 打开目标文件夹
        /// </summary>
        event EventHandler<ExecutionFolderRequestedEventArgs> ExecutionFolderRequested;
        event EventHandler VisibleChanged;

        bool IsVisible { get; }
        string RawInput { get; }

        void SetActions(ObservableCollection<ActionBase> actions);

        void HideWindow();
        void ShowWindow(IProgramContext context);
        void SetKeyDown(KeyEventHandler keyEventHandler);

        void ClearInput();

        void OnActionUpdateCompleted();
    }
}

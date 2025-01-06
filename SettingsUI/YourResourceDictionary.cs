using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using ReflectSettings.EditableConfigs;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace FrontendDemo
{
    public partial class YourResourceDictionary : ResourceDictionary
    {
        public YourResourceDictionary()
        {
            InitializeComponent();

        }

		private void OnButtonClick(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.Tag is string keyName)
			{
				MessageBox.Show($"Button clicked! KeyName: {keyName}");
			}
		}

		/// <summary>
		/// 取消掉下拉框的滚动事件，并返回给父布局
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                // 重置事件状态
                e.Handled = true;

                // 查找父级元素并触发滚轮事件
                var parent = comboBox.Parent as UIElement;
                if (parent != null)
                {
                    // 手动触发鼠标滚轮事件
                    var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = e.OriginalSource
                    };
                    parent.RaiseEvent(args);
                }
            }
        }

        private void TextBox_PreviewTextInput_IntType(object sender, TextCompositionEventArgs e)
        {
            // 检查输入是否为数字
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
        {
            return int.TryParse(text, out _); // 允许整数输入
        }

        private void TextBox_PreviewTextInput_DoubleType(object sender, TextCompositionEventArgs e)
        {
            // 检查输入是否为有效的浮点数
            e.Handled = !IsValidDoubleInput((sender as TextBox)?.Text, e.Text);
        }

        private bool IsValidDoubleInput(string currentText, string newText)
        {
            // 合并当前文本和新的输入
            string combinedText = currentText + newText;

            // 检查是否为有效的浮点数
            return double.TryParse(combinedText, out _);
        }

        
        protected void OnShortcutKeyEdit(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox) // 确保 sender 是一个 TextBox
            {
                ModifierKeys modifiers = Keyboard.Modifiers;
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;

                // 忽略修饰键单独按下的情况
                if (key != Key.None && key != Key.LeftCtrl && key != Key.RightCtrl &&
                    key != Key.LeftAlt && key != Key.RightAlt &&
                    key != Key.LeftShift && key != Key.RightShift)
                {
                    try
                    {
                        // 创建 KeyGesture 对象并转换为字符串
                        var shortcut = new KeyGesture(key, modifiers);
                        string shortcutText = shortcut.GetDisplayStringForCulture(System.Globalization.CultureInfo.CurrentCulture);
                        textBox.Text = shortcutText; // 显示友好的快捷键描述
                    }
                    catch (NotSupportedException)
                    {
                        textBox.Text = "Unsupported Shortcut"; // 如果仍然无效，提示用户
                    }
                    // textBox.Text = shortcut.ToString(); // 将快捷键显示在 TextBox 中
                    e.Handled = true; // 阻止事件进一步传播
                }
            }
        }
        
        private void BrowseButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender is Button button && button.DataContext is EditableString editableString)
            {
                // *.exe
                // 获取文件筛选器内容
                string filterValues =editableString.GetFilePathSelectorFilterValues();

                if (filterValues != null && filterValues == "Folder")
                {
                    // 使用 FolderBrowserDialog 来选择文件夹
                    // 使用 CommonOpenFileDialog 来选择文件夹
                    
                    // 使用 CommonOpenFileDialog 来选择文件夹
                    var dialog = new CommonOpenFileDialog
                    {
                        IsFolderPicker = true, // 启用文件夹选择模式
                        Title = "请选择一个文件夹"
                    };

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        // 更新绑定的数据对象的 Value 属性
                        editableString.Value = dialog.FileName;
                    }
                    
                }
                else
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    // openFileDialog.Filter = "All Files|*.*"; // 调整文件过滤器
                    // 将筛选内容设置为文件对话框的 Filter
                    openFileDialog.Filter = string.IsNullOrWhiteSpace(filterValues) ? "All Files|*.*" : filterValues;

                    if (openFileDialog.ShowDialog() == true)
                    {
                        // 更新绑定的数据对象的 Value 属性
                        editableString.Value = openFileDialog.FileName;
                    }

                }
                
            }
        }


    }
}

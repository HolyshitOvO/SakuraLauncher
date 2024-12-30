using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FrontendDemo
{
    public partial class YourResourceDictionary : ResourceDictionary
    {
        public YourResourceDictionary()
        {
            InitializeComponent();

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


    }
}

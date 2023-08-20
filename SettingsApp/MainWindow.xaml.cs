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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SettingsApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            navMenu.SelectedIndex = 0; // 设置第一项为默认选中

        }
        // 页面实例的缓存
        private static readonly Dictionary<Type, Page> bufferedPages =
            new Dictionary<Type, Page>();

        // 当
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 如果选择项不是 ListBoxItem, 则返回
            if (!(navMenu.SelectedItem is ListBoxItem item))
                return;
            if (navMenu.SelectedItem == null)
                return;
            // 如果 Tag 不是一个类型, 则返回
            if (!(item.Tag is Type type))
                return;
            
            // 如果页面缓存中找不到页面, 则创建一个新的页面并存入
            if (!bufferedPages.TryGetValue(type, out Page page))
                page = bufferedPages[type] =
                    Activator.CreateInstance(type) as Page ?? throw new Exception("this would never happen");

            // 使用 Frame 进行导航.
            appFrame.Navigate(page);
            
        }

        //private void NavigateToSelectedPage()
        //{
        //    if (navMenu.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is System.Type selectedPageType)
        //    {
        //        // 进行页面导航
        //        FrameMain.NavigationService.Navigate(Activator.CreateInstance(selectedPageType));
        //    }
        //}
    }
}

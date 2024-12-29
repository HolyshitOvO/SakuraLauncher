using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JetBrains.Annotations;
using Newtonsoft.Json;
using ReflectSettings;
using ReflectSettings.EditableConfigs;

namespace FrontendDemo
{
    public partial class MainWindow
    {
        private readonly ComplexConfiguration _complexConfiguration;
        [UsedImplicitly]
        public ObservableCollection<IEditableConfig> Editables { get; set; }

        private const string JsonFilePath = "Config.json";

        public MainWindow()
        {
            try
            {
                DataContext = this;
                Editables = new ObservableCollection<IEditableConfig>();
                InitializeComponent();
                var fac = new SettingsFactory();
                _complexConfiguration = new ComplexConfiguration();
                if (File.Exists(JsonFilePath))
                    _complexConfiguration = JsonConvert.DeserializeObject<ComplexConfiguration>(File.ReadAllText(JsonFilePath));

                var changeTrackingManager = new ChangeTrackingManager();
                var editableConfigs = fac.Reflect(_complexConfiguration, changeTrackingManager);

                foreach (var config in editableConfigs)
                {
                    Editables.Add(config);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var asJson = JsonConvert.SerializeObject(_complexConfiguration);
            File.WriteAllText(JsonFilePath, asJson);
        }

        private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                // 停止事件继续向父级控件传递
                e.Handled = true;
            }
        }

    }
}

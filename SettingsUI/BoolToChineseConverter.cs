using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FrontendDemo
{
    internal class BoolToChineseConverter : IValueConverter
    {
        public static BoolToChineseConverter Instance { get; } = new BoolToChineseConverter();

        private BoolToChineseConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? "开" : "关";
            }
            return "关";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return strValue == "开";
            }
            return false;
        }
    }
}

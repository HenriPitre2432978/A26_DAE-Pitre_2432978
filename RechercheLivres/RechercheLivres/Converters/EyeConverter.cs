using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using RechercheLivres.Model;
using RechercheLivres.ViewModels;

namespace RechercheLivres.Converters
{
    public class EyeIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is ActivityVM activity && values[1] is Dictionary<Activity, string> icons)
                return icons.TryGetValue(activity.Activity, out string icon) ? icon : "👁️";
            else if (values[0] is Expense e && values[1] is Dictionary<Expense, string> i)
                return i.TryGetValue(e, out string ico) ? ico : "👁️";
            return "👁️";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

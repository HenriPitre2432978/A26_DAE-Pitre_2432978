using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RechercheLivres.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        // conversion UI : inverse la valeur bool si valide, sinon false
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;
    }
}
 
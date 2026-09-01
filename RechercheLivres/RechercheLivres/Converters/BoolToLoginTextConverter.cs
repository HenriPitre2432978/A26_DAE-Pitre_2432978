using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RechercheLivres.Converters
{
    public class BoolToLoginTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isLoggedIn)
            {
                return isLoggedIn ? "Logout" : "Login";
            }
            return "ERROR";
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(); //pas de convert back needed
        }
    }
}

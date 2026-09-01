using System;
using System.Globalization;
using System.Windows.Data;
using RechercheLivres.Model;
using RechercheLivres.ViewModels;

namespace RechercheLivres.Converters
{
    public class OwnerToCrownConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[1] is not Participant user)
                return "";

            // Case 1: Activity
            if (values[0] is ActivityVM activity)
                return activity.Owner == user ? "👑" : "";

            // Case 2: Expense
            if (values[0] is Expense expense)
                return expense.Owner == user ? "👑" : "";

            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
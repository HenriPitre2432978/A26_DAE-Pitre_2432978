using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using RechercheLivres.Model;
using RechercheLivres.ViewModels;

namespace RechercheLivres.Converters
{
    public class BalanceToColorConverter : IMultiValueConverter
    {
        // values[0] = Expense or Activity
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[1] is not Participant user)
                return Brushes.Gray;

            // Handle Expense
            if (values[0] is Expense expense && values[2] is SearchListVM vm)
            {
                if (!expense.Participants.Contains(user) && !expense.Owner.Equals(user))
                    return Brushes.Gray;

                decimal remaining = vm.GetSolde(expense);
                if (remaining == 0) return Brushes.Gray;
                if (expense.Owner == user) return Brushes.Green;
                else return Brushes.Red;


            }

            // Handle Activity
            if (values[0] is ActivityVM activity)
            {
                decimal balance = activity.CurrentBalance;
                

                if (balance < 0) return Brushes.Red;      // user owes --> red
                if (balance > 0) return Brushes.Green;    // user is owed --> green
                return Brushes.Gray;                      // settled
            }

            return Brushes.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
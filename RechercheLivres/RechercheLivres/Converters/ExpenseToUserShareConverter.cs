using System;
using System.Globalization;
using System.Windows.Data;
using RechercheLivres.Model;
using RechercheLivres.ViewModels;
using System.Linq;

namespace RechercheLivres.Converters
{
    public class ExpenseToUserShareConverter : IMultiValueConverter
    {
        // values[0] = Expense
        // values[1] = Participant
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 || values[0] is not Expense expense || values[1] is not Participant user)
                return "❌";

            // Owner of the expense
            if (expense.Owner == user)
                return "❌Propriétaire";

            // Not in expense
            if (!expense.Participants.Contains(user))
                return "❌ Aucune";

            // Regular participant
            decimal share = expense.Amount / expense.Participants.Count;
            return share.ToString("C");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
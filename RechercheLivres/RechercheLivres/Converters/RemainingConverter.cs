using RechercheLivres.Model;
using RechercheLivres.ViewModels;
using System.Globalization;
using System.Windows.Data;

namespace RechercheLivres.Converters
{
    public class RemainingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Expense expense &&
    values[1] is SearchListVM vm)
            {
                Participant user = vm.User;

                if (expense.Owner == user)
                {
                    decimal owedByOthers = vm.GetSolde(expense); // déjà la somme totale
                    return owedByOthers > 0 ? $"➕{owedByOthers:C}" : "✔️ Réglé";
                }
                // Not involved or activity closed
                if (!expense.Participants.Contains(user) || !expense.IsActive)
                    return "❌ Aucun";


                decimal remaining = vm.GetSolde(expense);

                if (remaining < 0)
                    return $"➖ {Math.Abs(remaining):C}"; // YOU owe

                return "✔️ Réglé";
            }
            return "✔️ Réglé";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
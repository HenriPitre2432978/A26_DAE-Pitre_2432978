using System;
using System.Globalization;
using System.Windows.Data;
using RechercheLivres.Model;
using RechercheLivres.ViewModels;

namespace RechercheLivres.Converters
{
    public class UserBalanceToTextConverter : IMultiValueConverter
    {
        // convertisseur multi-binding : retourne un texte formaté selon le solde du user (rajoute un plus/- emoticone)
        // values[0] = Activity, values[1] = Participant
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //Validations
            if (values.Length < 2)
                return "";

            if (values[0] is not ActivityVM activity)
                return "";

            if (values[1] is not Participant user)
                return "";

            //Get balance
            decimal balance = activity.CurrentBalance;


            if (balance > 0)
                return $"➕ {balance:C}";

            if (balance < 0)
                return $"➖ {Math.Abs(balance):C}";

            //Balance nulle
            return "✔️ Réglé";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
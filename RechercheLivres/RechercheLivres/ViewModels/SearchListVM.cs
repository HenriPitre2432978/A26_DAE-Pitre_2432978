using RechercheLivres.Commands;
using RechercheLivres.Model;
using RechercheLivres.Service;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace RechercheLivres.ViewModels
{
    public class SearchListVM : BaseVM
    {

        #region  Propriétés

        private readonly INavigationService _navigation;
        private readonly Participant _user;
        private ObservableCollection<ActivityVM> _userActivities = [];

        #region Inputs and Get/Set properties

        private ActivityVM _SelActivity;
        public ActivityVM SelActivity
        {
            get => _SelActivity;
            set
            {
                _SelActivity = value;
                OnPropertyChanged(nameof(SelActivity));
                OnPropertyChanged(nameof(Expenses));

                SelectedExpense = SelActivity.Expenses.FirstOrDefault(e => e.Participants.Contains(User));

                RaiseCanExecuteChanged();
            }
        }

        private Expense _selectedExpense;
        public Expense SelectedExpense
        {
            get => _selectedExpense;
            set
            {
                _selectedExpense = value;
                OnPropertyChanged(nameof(SelectedExpense));

                UpdateDefaultAmount();
                RaiseCanExecuteChanged();
            }
        }

        private string _paymentAmountText = "";
        public string PaymentAmountText
        {
            get => _paymentAmountText;
            set
            {
                if (string.IsNullOrEmpty(value) || IsValidCashInput(value))
                {
                    _paymentAmountText = value;
                    OnPropertyChanged(nameof(PaymentAmountText));

                    _paymentAmount = ParseAmount(value);
                    OnPropertyChanged(nameof(PaymentAmount));

                    RaiseCanExecuteChanged();
                }
            }
        }

        private decimal _paymentAmount;
        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                _paymentAmount = value;
                _paymentAmountText = value.ToString(CultureInfo.InvariantCulture);
                OnPropertyChanged(nameof(PaymentAmount));
                OnPropertyChanged(nameof(PaymentAmountText));
                RaiseCanExecuteChanged();
            }
        }

        #endregion

        public Participant User => _user;
        public ObservableCollection<ActivityVM> UserActivities => _userActivities;
        public IEnumerable<Expense>? Expenses => SelActivity?.Expenses.Where(e => e.Participants.Contains(User));

        #endregion

        #region Commands
        public ICommand ShowParticipantsCommand { get; }

        public ICommand PayCommand { get; }
        public ICommand ReturnToOverviewCommand { get; }

        #endregion

        public SearchListVM(INavigationService nav, List<ActivityVM> activities, Participant user)
        {
            _navigation = nav;
            _user = user;

            foreach (ActivityVM a in activities)
                if (a.Participants.Contains(User) && a.IsActive)
                    _userActivities.Add(a);

            SelActivity = _userActivities.FirstOrDefault();

            PayCommand = new RelaisCommande(Pay, CanPay);
            ReturnToOverviewCommand = new RelaisCommande(() => _navigation.NavigateToOverview());

            ShowParticipantsCommand = new RelaisCommandeParametre(param =>
            {
                if (param is Expense expense)
                {
                    string names = string.Join("\n", expense.Participants.Select(p => p.Username));

                    MessageBox.Show(names, "Participants");
                }
            });
        }

        #region Methods

        #region Pay-Related methods
        private void Pay()
        {
            if (SelActivity == null || !SelActivity.IsActive) return;
            if (PaymentAmount <= 0) return;

            decimal remainingToPay = PaymentAmount;

            bool canPaySelected =
                SelectedExpense != null &&
                SelectedExpense.IsActive &&
                SelectedExpense.Owner != User;

            if (canPaySelected)
            {
                //Negative = what you owe
                decimal owed = Activity.GetRemainingForUser(SelectedExpense, User);

                if (owed > 0)
                {
                    decimal amount = Math.Min(owed, remainingToPay);
                    SelActivity.Activity.AddRefund(SelectedExpense, User, amount);
                    remainingToPay -= amount;
                }
            }

            // distributivité if amount was bigger than one expense
            if (remainingToPay
                > 0)
            {
                List<Expense> expenses = [.. SelActivity.Expenses
                    .Where(e => e.IsActive &&
                                e.Participants.Contains(User) &&
                                e != SelectedExpense)];

                foreach (Expense e in expenses)
                {
                    if (remainingToPay <= 0) break;

                    decimal owed = Activity.GetRemainingForUser(e, User);
                    if (owed <= 0) continue;

                    decimal amount = Math.Min(owed, remainingToPay);
                    SelActivity.Activity.AddRefund(e, User, amount);
                    remainingToPay -= amount;
                }
            }

            //Show amount leftover at the end
            PaymentAmount = remainingToPay;

            OnPropertyChanged(nameof(SelActivity.CurrentBalance));
            OnPropertyChanged(nameof(Expenses));
        }

        private bool CanPay()
        {
            if (SelActivity == null || !SelActivity.IsActive || SelActivity.CurrentBalance == 0)
                return false;

            if (PaymentAmount <= 0)
                return false;

            decimal remainingToPay = PaymentAmount;

            //Maximum possible de payer dans la dépense
            decimal maxPayable = Math.Abs(SelActivity.CurrentBalance);

            if (remainingToPay > maxPayable)
                return false;

            // If a specific expense is selected
            if (SelectedExpense != null)
            {
                if (!SelectedExpense.IsActive)
                    return false;

                if (SelectedExpense.Owner == User)
                    return false;

                decimal owed = Activity.GetRemainingForUser(SelectedExpense, User);

                return owed > 0;
            }

            // Otherwise check if any expense needs payment
            return SelActivity.Activity.Expenses.Any(e =>
                e.IsActive &&
                e.Participants.Contains(User) &&
                Activity.GetRemainingForUser(SelectedExpense, User) > 0);
        }

        #endregion

        #region Math/Logic Methods

        public decimal GetSolde(Expense e)
        {
            if (e == null || !e.IsActive || Activity.GetUserBalance(User, e.Parent) == 0) return 0;

            bool isOwner = e.Owner == User;
            int participantCount = e.Participants.Count;

            if (participantCount == 0) return 0;

            decimal share = e.Amount / participantCount;
            decimal solde = isOwner ? share * participantCount - share // total owed by others
                                    : -share; // participant owes their share

            // Subtract refunds made
            foreach (Refund r in SelActivity.Refunds.Where(r => r.Expense == e && (isOwner || r.Debitor == User)))
                solde += r.Amount;

            return solde;
        }

        /// <summary>
        /// Update the default input of an expense payment
        /// </summary>
        private void UpdateDefaultAmount()
        {
            if (SelectedExpense == null || SelActivity == null) return;

            PaymentAmount = Math.Round(Math.Min(Activity.GetRemainingForUser(SelectedExpense, User), Math.Abs(Activity.GetUserBalance(User, SelectedExpense.Parent))), 2);
        }

        #endregion

        #region Validity/Parsing/Functional methods

        private void RaiseCanExecuteChanged()
        {
            if (PayCommand is RelaisCommande cmd)
                cmd.RaiseCanExecuteChanged();
        }
        private static bool IsValidCashInput(string input)
        {
            // Allow digits, with at most one . or , as decimal separator
            // Regex: optional digits, optional separator, optional digits
            return Regex.IsMatch(input, @"^\d*[.,]?\d{0,2}$");
        }

        private static decimal ParseAmount(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;
            // Normalize: replace comma with dot for parsing
            var normalized = input.Replace(',', '.');
            return decimal.TryParse(normalized,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var result) ? result : 0;
        }
        #endregion

        #endregion

    }
}

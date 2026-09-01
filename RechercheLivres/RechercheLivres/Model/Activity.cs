using RechercheLivres.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xaml.Schema;

public class Activity
{

    #region Propriétés

    private string _name;

    private string _description;

    private Participant _owner;

    private ObservableCollection<Expense> _expenses = []; //initialisée dans constructeur

    private ObservableCollection<Participant> _participants = new(); //initialisée dans constructeur

    private ObservableCollection<Refund> _refunds = new();

    private bool _isActive;
    public bool IsActive => _isActive;


    #region Propriétés Get/Publiques/Readonly
    public string Name => _name;

    public string Description => _description;

    public Participant Owner => _owner;

    public decimal TotalAmount => Expenses.Sum(e => e.Amount);

    public ObservableCollection<Refund> Refunds => _refunds;
    public ObservableCollection<Expense> Expenses => _expenses;

    public ObservableCollection<Participant> Participants => _participants;


    #endregion

    #endregion

    public Activity(string name, Participant owner, List<Participant> participants, string? description = null)
    {
        _name = name;
        _owner = owner;

        _participants.Add(owner);
        foreach (Participant p in participants)
            if (!_participants.Contains(p)) //verif ajout doublon
                _participants.Add(p);


        _description = string.IsNullOrWhiteSpace(description) ? "Sans description" : description.Trim();
        _isActive = true; //TODO: Ajouter la possibilité de créer une activité inactive dès le départ (ex: pour préparer une activité à l'avance sans la rendre visible tout de suite)
    }

    #region Méthodes
    
    /// <summary>
    /// Invert this activity's Active state (IsActive)
    /// </summary>
    /// <returns>The new IsActive state</returns>
    public bool SwitchState() => _isActive = !_isActive;

    /// <summary>
    /// Rename this activity's name to this name
    /// </summary>
    public void Rename(string newName) => _name = newName;
    
    /// <summary>
    /// Updates the description with a new one from parameter.
    /// </summary>
    public void ReDescribe(string newDesc) => _description = newDesc;

    /// <summary>
    /// Add an expense object to the list of this acitivty's expenses
    /// </summary>
    /// <param name="expense">Expense to add</param>
    public void AddExpense(Expense expense) => _expenses.Add(expense);

    /// <summary>
    /// Add a Refund to this activity'S refund list by defining each property
    /// </summary>
    /// <param name="expense">Expense that's refunded</param>
    /// <param name="debitor">Person paying. Usually the User logged in</param>
    /// <param name="amount">Refund amount (can be partial)</param>
    public void AddRefund(Expense expense, Participant debitor, decimal amount)
    {
        if (!IsActive || !expense.IsActive) return;
        _refunds.Add(new Refund(this, expense, debitor, expense.Owner, amount));

    }
    /// <summary>
    /// Add a Participant object to this activity's Participants list 
    /// </summary>
    /// <param name="participant">Participant to add</param>
    public void AddParticipant(Participant participant)
    {
        if (!_participants.Contains(participant))
            _participants.Add(participant);
    }

    /// <summary>
    /// Static way to get an user's balance on an activity
    /// Difference with getremaining is this can be neg/pos/0
    /// </summary>
    /// <param name="user">user to check balance</param>
    /// <param name="a">activity to check balance</param>
    /// <returns>Decimal amount, negative or positive</returns>
    public static decimal GetUserBalance(Participant user, Activity a)
    {
        if (!a.IsActive) return 0;

        decimal bal = 0;

        foreach (Expense e in a.Expenses.Where(e => e.IsActive))
        {
            if (e.Owner == user)
            {
                bal += e.Participants
                    .Where(p => p != user)
                    .Sum(p => GetRemainingForUser(e, p));
            }
            else if (e.Participants.Contains(user))
            {
                bal -= GetRemainingForUser(e, user);
            }
        }

        return bal;
    }

    /// <summary>
    /// Get an User's absolute remaining balance (Math.Abs(bal))
    /// </summary>
    /// <param name="expense"></param>
    /// <param name="user">User to check</param>
    /// <param name="a">Activity to check</param>
    /// <returns>the decimal balance remaining, to the positive</returns>
    public static decimal GetRemainingForUser(Expense expense, Participant user)
    {
        if (expense == null || !expense.Parent.IsActive || !expense.IsActive) return 0;

        int participantCount = expense.Participants.Count;
        if (participantCount == 0) return 0;

        decimal baseShare = Math.Floor(expense.Amount / participantCount * 100) / 100; // floor to cents
        decimal totalFloor = baseShare * participantCount;
        decimal remainder = Math.Round(expense.Amount - totalFloor, 2); // leftover cents go to owner


        if (expense.Owner == user)
        {
            decimal owed = 0;
            foreach (Participant p in expense.Participants)
            {
                if (p == user) continue;
                decimal paidByParticipant = expense.Parent.Refunds
                    .Where(r => r.Expense == expense && r.Debitor == p)
                    .Sum(r => r.Amount);
                decimal remaining = baseShare - paidByParticipant;
                if (remaining > 0)          // ← was > 0.01m
                    owed += remaining;
            }
            return Math.Round(owed, 2, MidpointRounding.AwayFromZero);
        }

        if (expense.Participants.Contains(user))
        {
            decimal paidByUser = expense.Parent.Refunds
                .Where(r => r.Expense == expense && r.Debitor == user)
                .Sum(r => r.Amount);
            decimal remaining = baseShare - paidByUser;
            return remaining > 0 
                ? Math.Round(remaining, 2, MidpointRounding.AwayFromZero)
                : 0;
        }

        return 0;
    }
    #endregion
}
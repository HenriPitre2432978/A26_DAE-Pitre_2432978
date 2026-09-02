using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RechercheLivres.Commands
{
    public class RelaisCommande : ICommand
    {

        // Délégués pour l'exécution de la commande et la vérification de l'état exécutable
        private readonly Action _commande;

        // Délégué pour vérifier si la commande peut être exécutée
        private readonly Func<bool>? _executable;

        public RelaisCommande(Action commande, Func<bool>? executable = null)
        {
            _commande = commande;
            _executable = executable;
        }

        #region Implémentation de l'interface ICommand

        // Événement déclenché lorsque l'état exécutable de la commande change
        public event EventHandler? CanExecuteChanged;

        // Méthode pour vérifier si la commande peut être exécutée
        public bool CanExecute(object? parameter) => _executable == null || _executable();

        // Méthode pour exécuter la commande
        public void Execute(object? parameter) => _commande();


        public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        #endregion
    }
}

using System.Windows.Input;

namespace RechercheLivres.Commands
{
    public class RelaisCommandeParametre : ICommand
    {
        // Délégués pour l'exécution de la commande et la vérification de l'état exécutable
        private readonly Action<object> _execute;

        // Délégué pour vérifier si la commande peut être exécutée
        private readonly Predicate<object> _canExecute;

        public RelaisCommandeParametre(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Méthode pour vérifier si la commande peut être exécutée
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Méthode pour exécuter la commande, avec le paramètre passé
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        // Événement déclenché lorsque l'état exécutable de la commande change (ex pour rafraichir wpf)
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
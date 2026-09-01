using RechercheLivres.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace RechercheLivres.Service
{
    #region Interface de Navigation 

    //Une interface définit un contrat.
    //Tout class, record ou struct qui implémente ce contrat doit fournir une implémentation des membres définis dans l’interface.
    public interface INavigationService
    {
        BaseVM CurrentView { get; }

        void NavigateToOverview();
        void NavigateToNewActivity();
        void NavigateToConsultActivity(ActivityVM activity);
        void NavigateToNewExpense(ActivityVM activity);
        void NavigateToNewRefund();
        void NavigateToModifyParticipant(ActivityVM activity);
        void NavigateToCreateAccount();
        void NavigateToLogin();
    }

    #endregion

    //Classe qui découle de l'interface ci-haut, et qui implémente l'action de créer/naviguer entre les différentes instances de l'application.
    //TODO:  Vérifier si cette forme de navigation entraine une création en boucle de nouvelles instances. Si oui, est-ce que GarbageCollector le ramasse ?????
    public class NavigationService(MainVM main) : INavigationService
    {
        private BaseVM? _currentView;

        public BaseVM? CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                CurrentViewChanged?.Invoke(); //Un peu comme NotifyPropertyChanged, notifie que changement dans la CurrentView.
            }
        }

        public event Action? CurrentViewChanged;

        private readonly MainVM _main = main;

        //Toujours ajouter le lien VM --> View dans App.xaml.cs après la création d'une méthode de navigation 
        #region Méthodes distinctes pour chaque nouvelle navigation liée à un VM et à une vue

        public void NavigateToOverview() =>
     CurrentView = new OverviewVM(_main.ActivityVMs, _main.Participants, _main.User, this, _main);


        public void NavigateToNewActivity() =>
            CurrentView = new NewActivityVM(this, [.. _main.Participants], _main.User, _main.ActivityVMs);


        public void NavigateToConsultActivity(ActivityVM activity) =>
            CurrentView = new ConsultActivityVM(this, activity, _main.User, [.. _main.Participants]);


        public void NavigateToNewExpense(ActivityVM activity) =>
            CurrentView = new NewExpenseVM(this, activity, _main.User);

        public void NavigateToNewRefund() =>
      CurrentView = new SearchListVM(this, _main.ActivityVMs.ToList(), _main.User);

        public void NavigateToModifyParticipant(ActivityVM activity) =>
            CurrentView = new ModifyParticipantVM(this, [.. _main.Participants], activity);

        public void NavigateToLogin()
        {
            CurrentView = new LoginVM(this, _main.Participants, _main);
        }
        public void NavigateToCreateAccount()
        {
            //throw new NotImplementedException();
            CurrentView = new CreateAccountVM(this,_main.Participants,_main);
        }

        public void NavigateToMainWindow()
        {
            //Restart completely TODO le faire en effacant old MainVM ?
            CurrentView = null;

        }

        #endregion
    }
}

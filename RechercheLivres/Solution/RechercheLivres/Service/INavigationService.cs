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

        void NavigateToSearchList();
        void NavigateToMainWindow();
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

        public void NavigateToSearchList() =>
     CurrentView = new SearchListVM(_main.Livres,_main.Membres,_main.Emprunts);

        public void NavigateToMainWindow()
        {
            //Restart completely TODO le faire en effacant old MainVM ?
            CurrentView = null;

        }

        #endregion
    }
}

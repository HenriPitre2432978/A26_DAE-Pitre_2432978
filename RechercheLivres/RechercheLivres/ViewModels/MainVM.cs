using RechercheLivres.Commands;
using RechercheLivres.Model;
using RechercheLivres.Service;
using RechercheLivres.Service.SQLFetch;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace RechercheLivres.ViewModels
{
    public class MainVM : BaseVM
    {
        #region Properties

        #region Navigation

        //Instancier le service de changement de fenêtre
        private readonly NavigationService _navigation;

        //Définir la fenêtre actuelle
        public BaseVM CurrentView => _navigation.CurrentView;

        //Commande qui lance le changement (définie dans constructeur)
        public ICommand GoToSearchListCommand { get; }

        #endregion


        #region Project Properties

        //Collections qui seront populées dans SearchList
        public ObservableCollection<Livre> Livres { get; set; } = [];

        public ObservableCollection<Membre> Membres { get; set; } = [];

        public ObservableCollection<Emprunt> Emprunts { get; set; } = [];
        

        //Relative to current SearchList selected line
        public Livre? SelectedLivre { get; set; }

        public Membre? SelectedMembre { get; set; }

        public Emprunt? SelectedEmprunt { get; set; }

        #endregion

        #endregion

        #region Services de "désérialisation" SQL

        private readonly LivreService _livreService;

        private readonly MembreService _membreService;

        private readonly EmpruntService _empruntService;

        #endregion

        #region Constructor

        public MainVM()
        {
            // Initialisation des services
            _livreService = new LivreService();
            _membreService = new MembreService();
            _empruntService = new EmpruntService();

            // Initialisation de la navigation
            _navigation = new NavigationService(this);

            _navigation.CurrentViewChanged += () => { OnPropertyChanged(nameof(CurrentView)); };


            // Commande pour aller à la liste de recherche
            GoToSearchListCommand = new RelaisCommande(() => _navigation.NavigateToSearchList());

            // Chargement des données
            ChargerDonnees();
        }

        #endregion


        #region Peuplement (des données, pas les filles du roi)

        private void ChargerDonnees()
        {
            ChargerLivres();
            ChargerMembres();
            ChargerEmprunts();
        }


        private void ChargerLivres()
        {
            Livres.Clear();

            List<Livre> livres = _livreService.GetTousLivres();

            foreach (Livre livre in livres)
                Livres.Add(livre);
        }


        private void ChargerMembres()
        {
            Membres.Clear();

            List<Membre> membres = _membreService.GetTousMembres();

            foreach (Membre membre in membres)
                Membres.Add(membre);
        }


        private void ChargerEmprunts()
        {
            Emprunts.Clear();

            List<Emprunt> emprunts = _empruntService.GetTousEmprunts();

            foreach (Emprunt emprunt in emprunts)
                Emprunts.Add(emprunt);
        }

        #endregion


        #region Close

        public static bool Close()
        {

            MessageBoxResult result = MessageBox.Show(
                "Voulez-vous vraiment quitter l'application?",
                "Quitter",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        #endregion
    }
}
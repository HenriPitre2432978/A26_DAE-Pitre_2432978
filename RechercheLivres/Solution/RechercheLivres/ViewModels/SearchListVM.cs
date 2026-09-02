using RechercheLivres.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace RechercheLivres.ViewModels
{
    public class SearchListVM : BaseVM
    {
        #region Properties

        #region Collections affichées dans les listviews

        public ObservableCollection<Livre> Livres { get; }

        public ObservableCollection<Membre> Membres { get; }

        public ObservableCollection<Emprunt> Emprunts { get; }

        #endregion


        #region Views (Équivalent au Tabs xaml)

        public ICollectionView LivresView { get; }

        public ICollectionView MembresView { get; }

        public ICollectionView EmpruntsView { get; }

        #endregion

        #region Search PropertyChanged proeprties

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;

                _searchText = value;

                OnPropertyChanged(nameof(SearchText));

                ActualiserFiltres();
            }
        }

        #endregion

        #endregion

        #region Constructor

        public SearchListVM(ObservableCollection<Livre> livres,ObservableCollection<Membre> membres,ObservableCollection<Emprunt> emprunts)
        {
            //Assigner les collections chargées depuis MainVM into searchlistvm
            Livres = livres;
            Membres = membres;
            Emprunts = emprunts;

            // Création des vues filtrables
            LivresView = CollectionViewSource.GetDefaultView(Livres);
            MembresView = CollectionViewSource.GetDefaultView(Membres);
            EmpruntsView = CollectionViewSource.GetDefaultView(Emprunts);

            // Application des filtres aux views
            LivresView.Filter = FiltrerLivre;
            MembresView.Filter = FiltrerMembre;
            EmpruntsView.Filter = FiltrerEmprunt;
        }

        #endregion

        #region Filtering

        private void ActualiserFiltres()
        {
            LivresView.Refresh();
            MembresView.Refresh();
            EmpruntsView.Refresh();
        }

        private bool FiltrerLivre(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (obj is not Livre livre)
                return false;

            string rech = SearchText.Trim().ToLower();

            //Retourner true si titre/auteur/genre/isbn contient la recherche
            return
                (livre.Titre != null && livre.Titre.ToLower().Contains(rech))
                || (livre.Auteur != null && livre.Auteur.ToLower().Contains(rech))
                || (livre.Genre != null && livre.Genre.ToLower().Contains(rech))
                || (livre.ISBN != null && livre.ISBN.ToLower().Contains(rech));
        }


        private bool FiltrerMembre(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (obj is not Membre membre)
                return false;

            string recherche = SearchText.Trim().ToLower();

            //Retourner true si nom/email/telephone contient la recherche
            return
                (membre.Nom != null &&membre.Nom.ToLower().Contains(recherche))
                || (membre.Email != null && membre.Email.ToLower().Contains(recherche))
                || (membre.Telephone != null && membre.Telephone.ToLower().Contains(recherche));
        }


        private bool FiltrerEmprunt(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (obj is not Emprunt emprunt)
                return false;

            string recherche = SearchText.Trim().ToLower();

            //Retourner true si titre et/ou nomMembre contient la recherche
            return
                (emprunt.Titre != null && emprunt.Titre.ToLower().Contains(recherche))
                || (emprunt.NomMembre != null && emprunt.NomMembre.ToLower().Contains(recherche));
        }

        #endregion
    }
}
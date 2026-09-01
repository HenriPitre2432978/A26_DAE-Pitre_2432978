using RechercheLivres.Commands;
using RechercheLivres.Model;
using RechercheLivres.Service;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace RechercheLivres.ViewModels
{
    public class MainVM : BaseVM
    {

        #region Properties

        #region Functional Properties

        /// <summary>
        /// Save FilePath (Not in bin since project isn't compiled and is shared with sample data)
        /// </summary>
        private static readonly string DataFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "data.xml"));

        private NavigationService _navigation;
        public BaseVM CurrentView => _navigation.CurrentView;


        private bool _isWelcome = true;
        public bool IsWelcome
        {
            get => _isWelcome;
            set
            {
                _isWelcome = value;
                OnPropertyChanged(nameof(IsWelcome));
            }
        }

        #endregion

        #region Project Properties

        public Participant? _user = null;
        public Participant? User => _user;

        public ObservableCollection<ActivityVM> ActivityVMs { get; set; } = [];

        public ObservableCollection<Activity> Activities { get; set; }
        public ObservableCollection<Participant> Participants { get; set; }

        public Participant? SelectedParticipant { get; set; }
        public Activity? SelectedActivity { get; set; }

        #endregion

        public ICommand GoToLoginCommand { get; }

        public ICommand GoToCreateUserCommand { get; }

        #endregion

        public MainVM()
        {
            (Participants, Activities) = XMLService.Load(DataFilePath);

            _navigation = new NavigationService(this);

            _navigation.CurrentViewChanged += () =>
                OnPropertyChanged(nameof(CurrentView));

            GoToLoginCommand = new RelaisCommande(() =>
            {
                IsWelcome = false;
                _navigation.NavigateToLogin();
            });

            GoToCreateUserCommand = new RelaisCommande(() =>
            {
                IsWelcome = false;
                _navigation.NavigateToCreateAccount();
            });
        }

        #region User Login/Logout methods

        public string Login(string name, string pwd, IEnumerable<Participant> participants)
        {
            Participant? user = participants.FirstOrDefault(p => p.Username.ToLower() == name.ToLower()); //TODO: Check best: Mettre Participants en static ou passer participants en param in

            if (user == null) return "Utilisateur introuvable";


            // Auth
            if (!user.VerifyPassword(pwd)) return "Mot de passe invalide";

            //TODO: Théoriquement peut-être pas spécifier si utilisateur existe pas (plus sécuritaire....))

            _user = user;

            ActivityVMs = new ObservableCollection<ActivityVM>(Activities.Select(a => new ActivityVM(a, _user)));

            OnPropertyChanged(nameof(ActivityVMs));


            _navigation.NavigateToOverview();
            return string.Empty;
        }

        public void Logout()
        {
            _user = null;
            IsWelcome = true;
            _navigation.NavigateToMainWindow();
            Save();
        }

        #endregion

        #region Data Handling methods

        public void Save() =>
            XMLService.Save(DataFilePath, Participants, Activities);

        public bool Close()
        {
            if (!(_navigation.CurrentView is LoginVM || _navigation.CurrentView is CreateAccountVM  || _navigation.CurrentView == null))
            {
                var result = MessageBox.Show(
               "Voulez-vous enregistrer avant de quitter?",
               "Quitter",
               MessageBoxButton.YesNoCancel,
               MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel)
                    return false;

                if (result == MessageBoxResult.Yes)
                    XMLService.Save(DataFilePath, Participants, Activities);
            }
            
            return true;
        }

        #endregion
    
    }
}
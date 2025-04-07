using MemoryGame.View;
using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using MemoryGame.Commands;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryGame.ViewModel
{
    public class MenuWindowViewModel : INotifyPropertyChanged
    {
        private MemoryGame.Model.User _currentUser;

        public MenuWindowViewModel()
        {
            _currentUser = App.Current.Properties["CurrentUser"] as MemoryGame.Model.User;

            LogoutCommand = new RelayCommand(Logout);
            AboutCommand = new RelayCommand(ShowAbout);
            CloseAboutCommand = new RelayCommand(CloseAbout);
            NewGameCommand = new RelayCommand(StartNewGame);
            OpenGameCommand = new RelayCommand(OpenSavedGame);
            StatisticsCommand = new RelayCommand(ShowStatistics);
            CloseStatisticsCommand = new RelayCommand(CloseStatistics);
            OptionsCommand = new RelayCommand(ShowOptions);
            CloseOptionsCommand = new RelayCommand(CloseOptions);
            SaveOptionsCommand = new RelayCommand(SaveOptions);

            // Load game settings
            LoadGameSettings();

            UpdateStatisticsText();
            UpdateOpenGameState();
        }

        private Visibility _aboutVisibility = Visibility.Collapsed;
        public Visibility AboutVisibility
        {
            get => _aboutVisibility;
            set
            {
                _aboutVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _statisticsVisibility = Visibility.Collapsed;
        public Visibility StatisticsVisibility
        {
            get => _statisticsVisibility;
            set
            {
                _statisticsVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool _hasOpenGame;
        public bool HasSavedGame
        {
            get => _hasOpenGame;
            set
            {
                _hasOpenGame = value;
                OnPropertyChanged();
            }
        }

        private Visibility _optionsVisibility = Visibility.Collapsed;
        public Visibility OptionsVisibility
        {
            get => _optionsVisibility;
            set
            {
                _optionsVisibility = value;
                OnPropertyChanged();
            }
        }

        private int _timeLimit;
        public int TimeLimit
        {
            get => _timeLimit;
            set
            {
                _timeLimit = value;
                OnPropertyChanged();
                UpdateTotalCardsText();
                ValidateOptions();
            }
        }

        private int _gridRows;
        public int GridRows
        {
            get => _gridRows;
            set
            {
                _gridRows = value;
                OnPropertyChanged();
                UpdateTotalCardsText();
                ValidateOptions();
            }
        }

        private int _gridColumns;
        public int GridColumns
        {
            get => _gridColumns;
            set
            {
                _gridColumns = value;
                OnPropertyChanged();
                UpdateTotalCardsText();
                ValidateOptions();
            }
        }

        private string _totalCardsText;
        public string TotalCardsText
        {
            get => _totalCardsText;
            set
            {
                _totalCardsText = value;
                OnPropertyChanged();
            }
        }

        private string _optionsErrorMessage;
        public string OptionsErrorMessage
        {
            get => _optionsErrorMessage;
            set
            {
                _optionsErrorMessage = value;
                OnPropertyChanged();
            }
        }

        private Visibility _optionsErrorVisibility = Visibility.Collapsed;
        public Visibility OptionsErrorVisibility
        {
            get => _optionsErrorVisibility;
            set
            {
                _optionsErrorVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _statsPlayerName;
        public string StatsPlayerName
        {
            get => _statsPlayerName;
            set
            {
                _statsPlayerName = value;
                OnPropertyChanged();
            }
        }

        private string _statsGamesWon;
        public string StatsGamesWon
        {
            get => _statsGamesWon;
            set
            {
                _statsGamesWon = value;
                OnPropertyChanged();
            }
        }

        private string _statsGamesPlayed;
        public string StatsGamesPlayed
        {
            get => _statsGamesPlayed;
            set
            {
                _statsGamesPlayed = value;
                OnPropertyChanged();
            }
        }

        private string _statsWinRate;
        public string StatsWinRate
        {
            get => _statsWinRate;
            set
            {
                _statsWinRate = value;
                OnPropertyChanged();
            }
        }

        private string _savedGameInfo;
        public string SavedGameInfo
        {
            get => _savedGameInfo;
            set
            {
                _savedGameInfo = value;
                OnPropertyChanged();
            }
        }

        public ICommand StatisticsCommand { get; }
        public ICommand CloseStatisticsCommand { get; }

        private void UpdateStatisticsText()
        {
            if (_currentUser != null)
            {
                StatsPlayerName = $"Player: {_currentUser.Username}";
                StatsGamesWon = $"Games Won: {_currentUser.GamesWon}";
                StatsGamesPlayed = $"Games Played: {_currentUser.GamesPlayed}";
                StatsWinRate = $"Win Rate: {_currentUser.WinRate:F1}%";
            }
            else
            {
                StatsPlayerName = "No user logged in";
                StatsGamesWon = "Games Won: 0";
                StatsGamesPlayed = "Games Played: 0";
                StatsWinRate = "Win Rate: 0.0%";
            }
        }

        private void ShowStatistics()
        {
            UpdateStatisticsText();
            StatisticsVisibility = Visibility.Visible;
        }

        private void CloseStatistics()
        {
            StatisticsVisibility = Visibility.Collapsed;
        }

        public ICommand AboutCommand { get; }
        public ICommand CloseAboutCommand { get; }
        public ICommand OptionsCommand { get; }
        public ICommand CloseOptionsCommand { get; }
        public ICommand SaveOptionsCommand { get; }

        private void ShowAbout()
        {
            AboutVisibility = Visibility.Visible;
        }

        private void CloseAbout()
        {
            AboutVisibility = Visibility.Collapsed;
        }

        #region Options
        private void LoadGameSettings()
        {
            var settings = GameSettings.Instance;
            TimeLimit = settings.TimeLimit;
            GridRows = settings.Rows;
            GridColumns = settings.Columns;
            UpdateTotalCardsText();
        }

        private void ShowOptions()
        {
            // Reset to current settings
            LoadGameSettings();
            OptionsErrorVisibility = Visibility.Collapsed;
            OptionsVisibility = Visibility.Visible;
        }

        private void CloseOptions()
        {
            OptionsVisibility = Visibility.Collapsed;
        }

        private void SaveOptions()
        {
            if (!ValidateOptions())
            {
                return;
            }

            var settings = GameSettings.Instance;
            settings.TimeLimit = TimeLimit;
            settings.Rows = GridRows;
            settings.Columns = GridColumns;

            OptionsVisibility = Visibility.Collapsed;
        }

        private bool ValidateOptions()
        {
            int totalCards = GridRows * GridColumns;

            if (totalCards % 2 != 0)
            {
                OptionsErrorMessage = "The total number of cards (rows × columns) must be even to create pairs.";
                OptionsErrorVisibility = Visibility.Visible;
                return false;
            }

            OptionsErrorVisibility = Visibility.Collapsed;
            return true;
        }

        private void UpdateTotalCardsText()
        {
            int totalCards = GridRows * GridColumns;
            int pairs = totalCards / 2;
            TotalCardsText = $"Total: {totalCards} cards ({pairs} pairs)";
        }
        #endregion

        public ICommand NewGameCommand { get; }

        private void StartNewGame()
        {
            if (_currentUser?.SavedGameState != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Starting a new game will delete your saved game. Continue?",
                    "Confirm New Game",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                _currentUser.SavedGameState = null;
                App.Current.Properties["CurrentUser"] = _currentUser;
            }

            App.Current.Properties["LoadSavedGame"] = false;

            var menuWindow = Application.Current.Windows
                    .OfType<MenuWindow>()
                    .FirstOrDefault();
            GameWindow gameWindow = new GameWindow();
            gameWindow.Show();
            menuWindow.Close();
        }

        public ICommand OpenGameCommand { get; }

        private void UpdateOpenGameState()
        {
            HasSavedGame = _currentUser?.SavedGameState != null;

            if (HasSavedGame)
            {
                DateTime savedDate = _currentUser.SavedGameState.SavedDate;
                TimeSpan elapsed = DateTime.Now - savedDate;
                string timeAgo;

                if (elapsed.TotalDays >= 1)
                {
                    timeAgo = $"{(int)elapsed.TotalDays} days ago";
                }
                else if (elapsed.TotalHours >= 1)
                {
                    timeAgo = $"{(int)elapsed.TotalHours} hours ago";
                }
                else if (elapsed.TotalMinutes >= 1)
                {
                    timeAgo = $"{(int)elapsed.TotalMinutes} minutes ago";
                }
                else
                {
                    timeAgo = "just now";
                }

                SavedGameInfo = $"Last saved: {timeAgo}";
            }
            else
            {
                SavedGameInfo = "No saved game";
            }
        }

        private void OpenSavedGame()
        {
            if (_currentUser?.SavedGameState == null)
            {
                MessageBox.Show("No saved game found for this user.",
                    "No Saved Game",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            App.Current.Properties["LoadSavedGame"] = true;

            var menuWindow = Application.Current.Windows
                    .OfType<MenuWindow>()
                    .FirstOrDefault();
            GameWindow gameWindow = new GameWindow();
            gameWindow.Show();
            menuWindow.Close();
        }

        public ICommand LogoutCommand { get; }

        private void Logout()
        {
            var menuWindow = Application.Current.Windows
                    .OfType<MenuWindow>()
                    .FirstOrDefault();
            SignInWindow signInWindow = new SignInWindow();
            signInWindow.Show();
            menuWindow.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
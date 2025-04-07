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

        #region Constructor
        public MenuWindowViewModel()
        {
            // Get the current user from application properties
            _currentUser = App.Current.Properties["CurrentUser"] as MemoryGame.Model.User;

            // Initialize commands
            LogoutCommand = new RelayCommand(Logout);
            AboutCommand = new RelayCommand(ShowAbout);
            CloseAboutCommand = new RelayCommand(CloseAbout);
            NewGameCommand = new RelayCommand(StartNewGame);
            StatisticsCommand = new RelayCommand(ShowStatistics);
            CloseStatisticsCommand = new RelayCommand(CloseStatistics);

            // Initialize statistics texts if user is available
            UpdateStatisticsText();
        }
        #endregion

        #region Properties
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

        // Statistics text properties
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
        #endregion

        #region Statistics
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
            // Update the statistics text before showing the overlay
            UpdateStatisticsText();

            // Show the Statistics overlay
            StatisticsVisibility = Visibility.Visible;
        }

        private void CloseStatistics()
        {
            // Hide the Statistics overlay
            StatisticsVisibility = Visibility.Collapsed;
        }
        #endregion

        #region About
        public ICommand AboutCommand { get; }
        public ICommand CloseAboutCommand { get; }

        private void ShowAbout()
        {
            // Show the About overlay
            AboutVisibility = Visibility.Visible;
        }

        private void CloseAbout()
        {
            // Hide the About overlay
            AboutVisibility = Visibility.Collapsed;
        }
        #endregion

        #region New Game
        public ICommand NewGameCommand { get; }

        private void StartNewGame()
        {
            var menuWindow = Application.Current.Windows
                    .OfType<MenuWindow>()
                    .FirstOrDefault();
            GameWindow gameWindow = new GameWindow();
            gameWindow.Show();
            menuWindow.Close();
        }
        #endregion

        #region Logout
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
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
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
        #region Constructor
        public MenuWindowViewModel()
        {
            LogoutCommand = new RelayCommand(Logout);
            AboutCommand = new RelayCommand(ShowAbout);
            NewGameCommand = new RelayCommand(StartNewGame);
        }
        #endregion

        #region About
        public ICommand AboutCommand { get; }
        private void ShowAbout()
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
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
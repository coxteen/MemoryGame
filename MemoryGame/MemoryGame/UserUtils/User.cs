using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryGame.Model
{
    public class User : INotifyPropertyChanged
    {
        private string _username;
        private string _avatarPath;
        private int _gamesWon;
        private int _gamesPlayed;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string AvatarPath
        {
            get => _avatarPath;
            set
            {
                _avatarPath = value;
                OnPropertyChanged();
            }
        }

        public int GamesWon
        {
            get => _gamesWon;
            set
            {
                _gamesWon = value;
                OnPropertyChanged();
            }
        }

        public int GamesPlayed
        {
            get => _gamesPlayed;
            set
            {
                _gamesPlayed = value;
                OnPropertyChanged();
            }
        }

        public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100 : 0;

        public User(string username, string avatarPath, int gamesWon = 0, int gamesPlayed = 0)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            AvatarPath = avatarPath;
            GamesWon = gamesWon;
            GamesPlayed = gamesPlayed;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
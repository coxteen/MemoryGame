using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace MemoryGame.Model
{
    public class User : INotifyPropertyChanged
    {
        private string _username;
        private string _avatarPath;
        private int _gamesWon;
        private int _gamesPlayed;
        private SavedGameState _savedGameState;

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

        public SavedGameState SavedGameState
        {
            get => _savedGameState;
            set
            {
                _savedGameState = value;
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
            SavedGameState = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SavedGameState
    {
        public List<SavedCard> Cards { get; set; }
        public int TimeRemaining { get; set; }
        public int Moves { get; set; }
        public int GridRows { get; set; }
        public int GridColumns { get; set; }
        public DateTime SavedDate { get; set; }

        public SavedGameState()
        {
            Cards = new List<SavedCard>();
            SavedDate = DateTime.Now;
        }
    }

    public class SavedCard
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public bool IsMatched { get; set; }
        public bool IsFlipped { get; set; }
    }
}
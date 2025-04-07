using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryGame.Model
{
    public class GameSettings : INotifyPropertyChanged
    {
        private int _timeLimit = 60;
        private int _rows = 4;
        private int _columns = 4;
        private static GameSettings _instance;

        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSettings();
                }
                return _instance;
            }
        }

        private GameSettings()
        {
        }

        public int TimeLimit
        {
            get => _timeLimit;
            set
            {
                if (value < 10)
                    value = 10; // Minimum 10 seconds
                if (value > 300)
                    value = 300; // Maximum 5 minutes

                _timeLimit = value;
                OnPropertyChanged();
            }
        }

        public int Rows
        {
            get => _rows;
            set
            {
                if (value < 2)
                    value = 2; // Minimum 2 rows
                if (value > 8)
                    value = 8; // Maximum 8 rows

                _rows = value;
                OnPropertyChanged();
            }
        }

        public int Columns
        {
            get => _columns;
            set
            {
                if (value < 2)
                    value = 2; // Minimum 2 columns
                if (value > 8)
                    value = 8; // Maximum 8 columns

                _columns = value;
                OnPropertyChanged();
            }
        }

        public int TotalCards => Rows * Columns;

        public bool IsValidConfiguration()
        {
            // Check if total cards is even (required for pairs)
            return TotalCards % 2 == 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
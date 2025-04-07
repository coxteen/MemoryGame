using MemoryGame.Commands;
using MemoryGame.Model;
using MemoryGame.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MemoryGame.ViewModel
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _gameTimer;
        private readonly Random _random = new Random();
        private Card _firstSelectedCard;
        private Card _secondSelectedCard;
        private bool _isProcessingMatch;
        private readonly UserDataService _userDataService;
        private User _currentUser;
        private bool _isLoadedGame;

        public GameViewModel()
        {
            _userDataService = new UserDataService();
            _currentUser = App.Current.Properties["CurrentUser"] as User;

            BackToMenuCommand = new RelayCommand(BackToMenu);
            CardClickCommand = new RelayCommand<Card>(CardClick);
            SaveGameCommand = new RelayCommand(SaveGame);
            CloseSaveMessageCommand = new RelayCommand(CloseSaveMessage);

            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _gameTimer.Tick += GameTimer_Tick;

            bool loadSavedGame = false;
            if (App.Current.Properties.Contains("LoadSavedGame"))
            {
                loadSavedGame = (bool)App.Current.Properties["LoadSavedGame"];
                App.Current.Properties["LoadSavedGame"] = false;
            }

            if (loadSavedGame && _currentUser?.SavedGameState != null)
            {
                LoadSavedGame();
            }
            else
            {
                InitializeGame();
            }
        }

        private List<Card> _cards;
        public List<Card> Cards
        {
            get => _cards;
            set
            {
                _cards = value;
                OnPropertyChanged();
            }
        }

        private int _moves;
        public int Moves
        {
            get => _moves;
            set
            {
                _moves = value;
                OnPropertyChanged();
            }
        }

        private int _timeRemaining;
        public int TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
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
            }
        }

        private bool _gameOver;
        public bool GameOver
        {
            get => _gameOver;
            set
            {
                _gameOver = value;
                OnPropertyChanged();
            }
        }

        private string _gameMessage;
        public string GameMessage
        {
            get => _gameMessage;
            set
            {
                _gameMessage = value;
                OnPropertyChanged();
            }
        }

        private Visibility _messageVisibility = Visibility.Collapsed;
        public Visibility MessageVisibility
        {
            get => _messageVisibility;
            set
            {
                _messageVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _saveGameMessage;
        public string SaveGameMessage
        {
            get => _saveGameMessage;
            set
            {
                _saveGameMessage = value;
                OnPropertyChanged();
            }
        }

        private Visibility _saveMessageVisibility = Visibility.Collapsed;
        public Visibility SaveMessageVisibility
        {
            get => _saveMessageVisibility;
            set
            {
                _saveMessageVisibility = value;
                OnPropertyChanged();
            }
        }

        public ICommand BackToMenuCommand { get; }
        public ICommand CardClickCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand CloseSaveMessageCommand { get; }

        private void InitializeGame()
        {
            var settings = GameSettings.Instance;

            Moves = 0;
            TimeRemaining = settings.TimeLimit;
            GridRows = settings.Rows;
            GridColumns = settings.Columns;
            GameOver = false;
            GameMessage = string.Empty;
            MessageVisibility = Visibility.Collapsed;
            SaveMessageVisibility = Visibility.Collapsed;
            _firstSelectedCard = null;
            _secondSelectedCard = null;
            _isProcessingMatch = false;
            _isLoadedGame = false;

            LoadCards();
            _gameTimer.Start();
        }

        private void LoadSavedGame()
        {
            try
            {
                SavedGameState savedState = _currentUser.SavedGameState;
                if (savedState == null)
                {
                    InitializeGame();
                    return;
                }

                Moves = savedState.Moves;
                TimeRemaining = savedState.TimeRemaining;
                GameOver = false;
                GameMessage = string.Empty;
                MessageVisibility = Visibility.Collapsed;
                SaveMessageVisibility = Visibility.Collapsed;
                _firstSelectedCard = null;
                _secondSelectedCard = null;
                _isProcessingMatch = false;
                _isLoadedGame = true;

                Cards = new List<Card>();
                foreach (var savedCard in savedState.Cards)
                {
                    var card = new Card
                    {
                        Id = savedCard.Id,
                        ImagePath = savedCard.ImagePath,
                        IsMatched = savedCard.IsMatched,
                        IsFlipped = savedCard.IsFlipped
                    };
                    Cards.Add(card);
                }

                // Set grid dimensions from saved state if available
                if (savedState.GridRows > 0 && savedState.GridColumns > 0)
                {
                    GridRows = savedState.GridRows;
                    GridColumns = savedState.GridColumns;
                }
                else
                {
                    // Fallback: calculate based on card count
                    int totalCards = Cards.Count;

                    // Try to find a nice square-ish grid for the cards
                    int sqrt = (int)Math.Sqrt(totalCards);

                    if (totalCards % sqrt == 0)
                    {
                        // Perfect square or rectangle
                        GridRows = sqrt;
                        GridColumns = totalCards / sqrt;
                    }
                    else
                    {
                        // Find factors
                        for (int i = sqrt; i >= 1; i--)
                        {
                            if (totalCards % i == 0)
                            {
                                GridRows = i;
                                GridColumns = totalCards / i;
                                break;
                            }
                        }
                    }
                }

                _gameTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading saved game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                InitializeGame();
            }
        }

        private void LoadCards()
        {
            try
            {
                string imagePath = @"C:\Stuff\Programming\University\Second Year\MAP\MemoryGame\MemoryGame\MemoryGame\res\images\categories\musicians\";

                var imageFiles = Directory.GetFiles(imagePath, "*.jpg")
                    .Concat(Directory.GetFiles(imagePath, "*.png"))
                    .Concat(Directory.GetFiles(imagePath, "*.jpeg"))
                    .ToList();

                // Calculate how many pairs we need based on grid dimensions
                int totalCards = GridRows * GridColumns;
                int pairsNeeded = totalCards / 2;

                // Make sure we have enough images
                if (imageFiles.Count < pairsNeeded)
                {
                    MessageBox.Show($"Not enough images found. Found {imageFiles.Count} but need {pairsNeeded}.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Use available images and repeat if necessary
                    while (imageFiles.Count < pairsNeeded)
                    {
                        imageFiles.Add(imageFiles[_random.Next(imageFiles.Count)]);
                    }
                }

                // Select random images for pairs
                imageFiles = imageFiles.OrderBy(x => _random.Next()).Take(pairsNeeded).ToList();

                Cards = new List<Card>();
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    string imagePath1 = imageFiles[i];

                    var card1 = new Card
                    {
                        Id = i * 2,
                        ImagePath = imagePath1,
                        IsMatched = false,
                        IsFlipped = false
                    };

                    var card2 = new Card
                    {
                        Id = i * 2 + 1,
                        ImagePath = imagePath1,
                        IsMatched = false,
                        IsFlipped = false
                    };

                    Cards.Add(card1);
                    Cards.Add(card2);
                }

                Cards = Cards.OrderBy(x => _random.Next()).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading images: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            TimeRemaining--;

            if (TimeRemaining <= 0)
            {
                EndGame(false);
            }
        }

        private void EndGame(bool isWin)
        {
            _gameTimer.Stop();
            GameOver = true;

            if (_currentUser != null)
            {
                _currentUser.GamesPlayed++;

                if (isWin)
                {
                    _currentUser.GamesWon++;
                }

                _currentUser.SavedGameState = null;
                _userDataService.SaveUser(_currentUser);
            }

            if (isWin)
            {
                GameMessage = "Congratulations! You've matched all cards!";
            }
            else
            {
                GameMessage = "Time's up! Game over.";
            }

            MessageVisibility = Visibility.Visible;
        }

        private void CheckWinCondition()
        {
            if (Cards.All(c => c.IsMatched))
            {
                EndGame(true);
            }
        }

        private void CardClick(Card card)
        {
            if (_isProcessingMatch || GameOver || card.IsMatched || card.IsFlipped)
            {
                return;
            }

            card.IsFlipped = true;

            if (_firstSelectedCard == null)
            {
                _firstSelectedCard = card;
                return;
            }

            if (_secondSelectedCard == null && card.Id != _firstSelectedCard.Id)
            {
                _secondSelectedCard = card;
                Moves++;

                _isProcessingMatch = true;

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_firstSelectedCard.ImagePath == _secondSelectedCard.ImagePath)
                    {
                        _firstSelectedCard.IsMatched = true;
                        _secondSelectedCard.IsMatched = true;

                        _firstSelectedCard = null;
                        _secondSelectedCard = null;
                        _isProcessingMatch = false;

                        CheckWinCondition();
                    }
                    else
                    {
                        DispatcherTimer wrongMatchTimer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(1)
                        };

                        wrongMatchTimer.Tick += (sender, e) =>
                        {
                            _firstSelectedCard.IsFlipped = false;
                            _secondSelectedCard.IsFlipped = false;

                            _firstSelectedCard = null;
                            _secondSelectedCard = null;
                            _isProcessingMatch = false;

                            wrongMatchTimer.Stop();
                        };

                        wrongMatchTimer.Start();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }
        }

        private void SaveGame()
        {
            if (GameOver)
            {
                SaveGameMessage = "Cannot save a finished game!";
                SaveMessageVisibility = Visibility.Visible;
                return;
            }

            _gameTimer.Stop();

            try
            {
                var savedState = new SavedGameState
                {
                    Cards = Cards.Select(c => new SavedCard
                    {
                        Id = c.Id,
                        ImagePath = c.ImagePath,
                        IsMatched = c.IsMatched,
                        IsFlipped = c.IsFlipped
                    }).ToList(),
                    TimeRemaining = TimeRemaining,
                    Moves = Moves,
                    GridRows = GridRows,
                    GridColumns = GridColumns,
                    SavedDate = DateTime.Now
                };

                if (_currentUser != null)
                {
                    _currentUser.SavedGameState = savedState;
                    _userDataService.SaveUser(_currentUser);

                    SaveGameMessage = "Game saved successfully!";
                }
                else
                {
                    SaveGameMessage = "Error: No user logged in!";
                }
            }
            catch (Exception ex)
            {
                SaveGameMessage = $"Error saving game: {ex.Message}";
            }

            SaveMessageVisibility = Visibility.Visible;
        }

        private void CloseSaveMessage()
        {
            SaveMessageVisibility = Visibility.Collapsed;

            if (!GameOver)
            {
                _gameTimer.Start();
            }
        }

        private void BackToMenu()
        {
            _gameTimer.Stop();

            var gameWindow = Application.Current.Windows
                .OfType<View.GameWindow>()
                .FirstOrDefault();

            View.MenuWindow menuWindow = new View.MenuWindow();
            menuWindow.Show();

            if (gameWindow != null)
            {
                gameWindow.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
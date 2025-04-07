using MemoryGame.Commands;
using MemoryGame.Model;
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

        #region Constructor

        public GameViewModel()
        {
            // Initialize commands
            BackToMenuCommand = new RelayCommand(BackToMenu);
            CardClickCommand = new RelayCommand<Card>(CardClick);

            // Create game timer
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _gameTimer.Tick += GameTimer_Tick;

            // Initialize game
            InitializeGame();
        }

        #endregion

        #region Game Properties

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

        private int _timeRemaining = 60; // 60 seconds by default
        public int TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
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

        #endregion

        #region Commands

        public ICommand BackToMenuCommand { get; }
        public ICommand CardClickCommand { get; }

        #endregion

        #region Game Methods

        private void InitializeGame()
        {
            // Reset game state
            Moves = 0;
            TimeRemaining = 60;
            GameOver = false;
            GameMessage = string.Empty;
            MessageVisibility = Visibility.Collapsed;
            _firstSelectedCard = null;
            _secondSelectedCard = null;
            _isProcessingMatch = false;

            // Load and prepare cards
            LoadCards();

            // Start the timer
            _gameTimer.Start();
        }

        private void LoadCards()
        {
            try
            {
                string imagePath = @"C:\Stuff\Programming\University\Second Year\MAP\MemoryGame\MemoryGame\MemoryGame\res\images\categories\musicians\";

                // Get all image files
                var imageFiles = Directory.GetFiles(imagePath, "*.jpg")
                    .Concat(Directory.GetFiles(imagePath, "*.png"))
                    .Concat(Directory.GetFiles(imagePath, "*.jpeg"))
                    .ToList();

                // Shuffle and select 8 images (for 16 cards / 8 pairs)
                imageFiles = imageFiles.OrderBy(x => _random.Next()).Take(8).ToList();

                // Create pairs of cards
                Cards = new List<Card>();
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    string imagePath1 = imageFiles[i];

                    // Create first card
                    var card1 = new Card
                    {
                        Id = i * 2,
                        ImagePath = imagePath1,
                        IsMatched = false,
                        IsFlipped = false
                    };

                    // Create second card (matching pair)
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

                // Shuffle the cards
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
                // Game over - time's up
                EndGame(false);
            }
        }

        private void EndGame(bool isWin)
        {
            _gameTimer.Stop();
            GameOver = true;

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
            // Ignore clicks if processing a match, game is over, card is already matched or flipped
            if (_isProcessingMatch || GameOver || card.IsMatched || card.IsFlipped)
            {
                return;
            }

            // Flip the card
            card.IsFlipped = true;

            // First card selection
            if (_firstSelectedCard == null)
            {
                _firstSelectedCard = card;
                return;
            }

            // Second card selection
            if (_secondSelectedCard == null && card.Id != _firstSelectedCard.Id)
            {
                _secondSelectedCard = card;
                Moves++;

                // Check for match
                _isProcessingMatch = true;

                // Using a dispatcher to delay the check to allow card animation to complete
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_firstSelectedCard.ImagePath == _secondSelectedCard.ImagePath)
                    {
                        // Match found
                        _firstSelectedCard.IsMatched = true;
                        _secondSelectedCard.IsMatched = true;

                        // Reset selections immediately for matches
                        _firstSelectedCard = null;
                        _secondSelectedCard = null;
                        _isProcessingMatch = false;

                        // Check if all cards are matched
                        CheckWinCondition();
                    }
                    else
                    {
                        // No match, show cards for 2 seconds before flipping back
                        // Use a timer to delay flipping back
                        DispatcherTimer wrongMatchTimer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(1)
                        };

                        wrongMatchTimer.Tick += (sender, e) =>
                        {
                            // Flip cards back
                            _firstSelectedCard.IsFlipped = false;
                            _secondSelectedCard.IsFlipped = false;

                            // Reset selections
                            _firstSelectedCard = null;
                            _secondSelectedCard = null;
                            _isProcessingMatch = false;

                            // Stop the timer
                            wrongMatchTimer.Stop();
                        };

                        // Start the timer
                        wrongMatchTimer.Start();
                    }
                }), DispatcherPriority.Background, new object[] { });
            }
        }

        private void BackToMenu()
        {
            // Stop the timer
            _gameTimer.Stop();

            // Open menu window and close game window
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

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    // Card Model
    public class Card : INotifyPropertyChanged
    {
        private int _id;
        private string _imagePath;
        private bool _isFlipped;
        private bool _isMatched;
        private BitmapImage _imageSource;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                LoadImageSource();
                OnPropertyChanged();
            }
        }

        public BitmapImage ImageSource
        {
            get => _imageSource;
            private set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                OnPropertyChanged();
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                _isMatched = value;
                OnPropertyChanged();
            }
        }

        private void LoadImageSource()
        {
            try
            {
                if (!string.IsNullOrEmpty(ImagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(ImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    ImageSource = bitmap;
                }
            }
            catch (Exception)
            {
                ImageSource = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
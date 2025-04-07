using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MemoryGame.Commands;
using MemoryGame.Model;
using MemoryGame.Services;
using MemoryGame.View;

namespace MemoryGame.ViewModel
{
    public class SignInViewModel : INotifyPropertyChanged
    {
        private readonly UserDataService _userDataService;

        #region Constructor
        public SignInViewModel()
        {
            _userDataService = new UserDataService();

            _avatarFiles = Directory.GetFiles(AvatarPath, "*.jpg")
                .Concat(Directory.GetFiles(AvatarPath, "*.png"))
                .Concat(Directory.GetFiles(AvatarPath, "*.jpeg"))
                .ToList();
            _currentAvatarIndex = 0;
            LoadCurrentAvatar();

            Users = _userDataService.LoadUsers();

            SelectedUser = Users.FirstOrDefault();

            // Initialize commands
            NextAvatarCommand = new RelayCommand(NextAvatar, CanNavigateAvatar);
            PreviousAvatarCommand = new RelayCommand(PreviousAvatar, CanNavigateAvatar);
            PlayCommand = new RelayCommand(Play, CanPlayGame);
            NewUserCommand = new RelayCommand(ShowNewUserDialog);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser);
            ExitCommand = new RelayCommand(Exit);
            SaveNewUserCommand = new RelayCommand(SaveNewUser, CanSaveNewUser);
            CancelNewUserCommand = new RelayCommand(CancelNewUser);
        }
        #endregion

        #region Properties
        private Visibility _newUserDialogVisibility = Visibility.Collapsed;
        public Visibility NewUserDialogVisibility
        {
            get => _newUserDialogVisibility;
            set
            {
                _newUserDialogVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _newUsername;
        public string NewUsername
        {
            get => _newUsername;
            set
            {
                _newUsername = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }
        #endregion

        #region User Management
        private User _selectedUser;
        public ObservableCollection<User> Users { get; }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand NewUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand SaveNewUserCommand { get; }
        public ICommand CancelNewUserCommand { get; }

        private void Exit()
        {
            // Properly shutdown the application
            Application.Current.Shutdown();
        }

        private void ShowNewUserDialog()
        {
            // Clear previous input and show dialog
            NewUsername = string.Empty;
            NewUserDialogVisibility = Visibility.Visible;
        }

        private void CancelNewUser()
        {
            // Hide the dialog
            NewUserDialogVisibility = Visibility.Collapsed;
        }

        private bool CanSaveNewUser()
        {
            return !string.IsNullOrWhiteSpace(NewUsername);
        }

        private void SaveNewUser()
        {
            if (string.IsNullOrWhiteSpace(NewUsername))
                return;

            var username = NewUsername.Trim();

            // Check if username already exists
            if (Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("A user with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create new user with current avatar
            var newUser = new User(username, _avatarFiles[_currentAvatarIndex]);
            Users.Add(newUser);
            SelectedUser = newUser;

            // Save user to settings
            _userDataService.SaveUser(newUser);

            // Hide the dialog
            NewUserDialogVisibility = Visibility.Collapsed;
        }

        private void DeleteUser()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete user '{SelectedUser.Username}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string username = SelectedUser.Username;
                Users.Remove(SelectedUser);
                SelectedUser = Users.FirstOrDefault();

                // Delete user from settings
                _userDataService.DeleteUser(username);
            }
        }

        private bool CanDeleteUser()
        {
            return SelectedUser != null;
        }
        #endregion

        #region Avatar Navigation
        private const string AvatarPath = @"C:\Stuff\Programming\University\Second Year\MAP\MemoryGame\MemoryGame\MemoryGame\res\images\avatars";
        private List<string> _avatarFiles;
        private int _currentAvatarIndex;
        private BitmapImage _currentAvatar;
        public BitmapImage CurrentAvatar
        {
            get => _currentAvatar;
            private set
            {
                _currentAvatar = value;
                OnPropertyChanged();
            }
        }
        public ICommand NextAvatarCommand { get; }
        public ICommand PreviousAvatarCommand { get; }
        private void LoadCurrentAvatar()
        {
            if (_avatarFiles == null || _avatarFiles.Count == 0)
            {
                CurrentAvatar = null;
                return;
            }
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_avatarFiles[_currentAvatarIndex], UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                CurrentAvatar = bitmap;
            }
            catch (Exception ex)
            {
                CurrentAvatar = null;
            }
        }
        private void NextAvatar()
        {
            _currentAvatarIndex = (_currentAvatarIndex + 1) % _avatarFiles.Count;
            LoadCurrentAvatar();
            UpdateSelectedUserAvatar();
        }
        private void PreviousAvatar()
        {
            _currentAvatarIndex = (_currentAvatarIndex - 1 + _avatarFiles.Count) % _avatarFiles.Count;
            LoadCurrentAvatar();
            UpdateSelectedUserAvatar();
        }
        private bool CanNavigateAvatar()
        {
            return _avatarFiles != null && _avatarFiles.Count > 1;
        }

        private void UpdateSelectedUserAvatar()
        {
            if (SelectedUser != null && _avatarFiles != null && _avatarFiles.Count > 0)
            {
                // Update the selected user's avatar path
                SelectedUser.AvatarPath = _avatarFiles[_currentAvatarIndex];

                // Save the updated user
                _userDataService.SaveUser(SelectedUser);
            }
        }
        #endregion

        #region Play
        public ICommand PlayCommand { get; }
        private bool CanPlayGame()
        {
            return SelectedUser != null;
        }
        private void Play()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Please select a user to play.", "No User Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var signInWindow = Application.Current.Windows
                    .OfType<SignInWindow>()
                    .FirstOrDefault();
            MenuWindow menuWindow = new MenuWindow();
            menuWindow.Show();
            signInWindow.Close();
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
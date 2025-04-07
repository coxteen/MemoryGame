using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MemoryGame.Services
{
    public class UserDataService
    {
        private readonly string _jsonFilePath;

        public UserDataService()
        {
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MemoryGame");

            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            _jsonFilePath = Path.Combine(appDataFolder, "C:\\Stuff\\Programming\\University\\Second Year\\MAP\\MemoryGame\\MemoryGame\\MemoryGame\\res\\userInfo\\users.json");
            Console.WriteLine($"Using JSON file: {_jsonFilePath}");
        }

        public ObservableCollection<User> LoadUsers()
        {
            var users = new ObservableCollection<User>();

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    Console.WriteLine("JSON file does not exist yet, returning empty user list");
                    return users;
                }

                string json = File.ReadAllText(_jsonFilePath);
                List<UserDto> userDtos = JsonSerializer.Deserialize<List<UserDto>>(json);

                // Convert DTOs to User objects
                foreach (var dto in userDtos)
                {
                    try
                    {
                        var user = new User(dto.Username, dto.AvatarPath, dto.GamesWon, dto.GamesPlayed);
                        users.Add(user);
                        Console.WriteLine($"Loaded user: {dto.Username}, Avatar: {dto.AvatarPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading user {dto.Username}: {ex.Message}");
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users from JSON: {ex.Message}");
                return new ObservableCollection<User>();
            }
        }

        public bool SaveUsers(IEnumerable<User> users)
        {
            try
            {
                var userDtos = users.Select(u => new UserDto
                {
                    Username = u.Username,
                    AvatarPath = u.AvatarPath,
                    GamesWon = u.GamesWon,
                    GamesPlayed = u.GamesPlayed
                }).ToList();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(userDtos, options);

                File.WriteAllText(_jsonFilePath, json);
                Console.WriteLine($"Saved {userDtos.Count} users to JSON file");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users to JSON: {ex.Message}");
                return false;
            }
        }

        public bool SaveUser(User user)
        {
            var users = LoadUsers();

            int index = -1;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Username == user.Username)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                users[index] = user;
            }
            else
            {
                users.Add(user);
            }

            return SaveUsers(users);
        }

        public bool DeleteUser(string username)
        {
            var users = LoadUsers();

            User userToRemove = users.FirstOrDefault(u => u.Username == username);
            if (userToRemove != null)
            {
                users.Remove(userToRemove);
                return SaveUsers(users);
            }

            return false;
        }

        private class UserDto
        {
            public string Username { get; set; }
            public string AvatarPath { get; set; }
            public int GamesWon { get; set; }
            public int GamesPlayed { get; set; }
        }
    }
}
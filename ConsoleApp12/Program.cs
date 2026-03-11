using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp12
{

    class Program
    {
        static List<PlayerProfile> players = new List<PlayerProfile>();
        static string playersFilePath = "players.txt";
        static string leaderboardFilePath = "leaderboard.txt";
        static PlayerProfile currentPlayer;
        static int[] ranges = { 0, 10, 50, 100, 250, 1000 };

        static void Main(string[] args)
        {
            LoadPlayers();

            Console.Write("Введите имя игрока: ");
            string name = Console.ReadLine().Trim();

            currentPlayer = null;
            for (int i = 0; i < players.Count; i++)
                if (players[i].PlayerName.ToLower() == name.ToLower())
                    currentPlayer = players[i];

            if (currentPlayer == null)
            {
                currentPlayer = new PlayerProfile(name, 1, 0);
                players.Add(currentPlayer);
                SavePlayers();
                Console.WriteLine("Новый игрок создан");
            }

            bool play = true;
            while (play)
            {
                Console.WriteLine($"Доступные уровни: 1..{currentPlayer.MaxLevel}");

                int lvl = 0;
                while (lvl < 1 || lvl > currentPlayer.MaxLevel)
                {
                    Console.Write($"Введите уровень (1..{currentPlayer.MaxLevel}): ");
                    string inp = Console.ReadLine().Trim();
                    if (!int.TryParse(inp, out lvl) || lvl < 1 || lvl > currentPlayer.MaxLevel)
                        Console.WriteLine($"Ошибка введите от 1 до {currentPlayer.MaxLevel}");
                }

                PlayGame(lvl);

                Console.WriteLine($"Текущие данные: Игрок {currentPlayer.PlayerName}, Макс. уровень {currentPlayer.MaxLevel}, Счёт {currentPlayer.Score}.");

                Console.WriteLine("1 - Выбрать уровень, 2 - Выйти");
                string ch = Console.ReadLine().Trim();
                if (ch == "2") play = false;
                else if (ch != "1") play = false;
            }

            Console.WriteLine("Уровень сложности задания: 3");
            ShowLeaderboard();
            SavePlayers();
        }

        static void LoadPlayers()
        {
            if (!File.Exists(playersFilePath)) return;

            string[] lines = File.ReadAllLines(playersFilePath);
            PlayerProfile p = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line == "---") { p = null; continue; }

                if (line.StartsWith("PlayerName:"))
                {
                    string n = line.Substring(11).Trim();
                    p = new PlayerProfile(n);
                    players.Add(p);
                }
                else if (line.StartsWith("MaxLevel:") && p != null)
                {
                    int.TryParse(line.Substring(9).Trim(), out int l);
                    p.MaxLevel = l < 1 ? 1 : (l > 5 ? 5 : l);
                }
                else if (line.StartsWith("Score:") && p != null)
                {
                    int.TryParse(line.Substring(6).Trim(), out int s);
                    p.Score = s < 0 ? 0 : s;
                }
            }
        }

        static void SavePlayers()
        {
            using (StreamWriter w = new StreamWriter(playersFilePath))
            {
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    w.WriteLine($"PlayerName: {p.PlayerName}");
                    w.WriteLine($"MaxLevel: {p.MaxLevel}");
                    w.WriteLine($"Score: {p.Score}");
                    if (i < players.Count - 1) w.WriteLine("---");
                }
            }
        }

        static void PlayGame(int lvl)
        {
            int max = ranges[lvl];
            Random r = new Random();
            int secret = r.Next(1, max + 1);
            int guess = 0;

            Console.WriteLine($"Загадано число от 1 до {max}");

            while (guess != secret)
            {
                Console.Write("Ваше предположение: ");
                string inp = Console.ReadLine().Trim();
                if (!int.TryParse(inp, out guess))
                {
                    Console.WriteLine("Надо число");
                    continue;
                }

                if (guess < secret) Console.WriteLine("Больше");
                else if (guess > secret) Console.WriteLine("Меньше");
            }

            Console.WriteLine("Угадал!");

            int points = lvl * lvl * 10;
            currentPlayer.Score += points;
            Console.WriteLine($"Получил {points} очков");

            if (lvl == currentPlayer.MaxLevel && currentPlayer.MaxLevel < 5)
            {
                currentPlayer.MaxLevel++;
                Console.WriteLine("Уровень повышен");
            }

            SaveToLeaderboard();
            SavePlayers();
        }

        static void SaveToLeaderboard()
        {
            List<LeaderboardEntry> lb = new List<LeaderboardEntry>();

            if (File.Exists(leaderboardFilePath))
            {
                string[] lines = File.ReadAllLines(leaderboardFilePath);
                LeaderboardEntry e = null;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line == "---")
                    {
                        if (e != null) lb.Add(e);
                        e = null;
                        continue;
                    }

                    if (line.StartsWith("PlayerName:"))
                    {
                        if (e != null) lb.Add(e);
                        string n = line.Substring(11).Trim();
                        e = new LeaderboardEntry();
                        e.PlayerName = n;
                    }
                    else if (line.StartsWith("Level:") && e != null)
                    {
                        int.TryParse(line.Substring(6).Trim(), out int l);
                        e.Level = l;
                    }
                    else if (line.StartsWith("Score:") && e != null)
                    {
                        int.TryParse(line.Substring(6).Trim(), out int s);
                        e.Score = s;
                    }
                }
                if (e != null) lb.Add(e);
            }

            bool found = false;
            for (int i = 0; i < lb.Count; i++)
            {
                if (lb[i].PlayerName.ToLower() == currentPlayer.PlayerName.ToLower())
                {
                    lb[i].Level = currentPlayer.MaxLevel;
                    lb[i].Score = currentPlayer.Score;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                lb.Add(new LeaderboardEntry
                {
                    PlayerName = currentPlayer.PlayerName,
                    Level = currentPlayer.MaxLevel,
                    Score = currentPlayer.Score
                });
            }

            for (int i = 0; i < lb.Count - 1; i++)
            {
                for (int j = i + 1; j < lb.Count; j++)
                {
                    if (lb[j].Score > lb[i].Score)
                    {
                        var temp = lb[i];
                        lb[i] = lb[j];
                        lb[j] = temp;
                    }
                }
            }

            if (lb.Count > 10) lb = lb.GetRange(0, 10);

            using (StreamWriter w = new StreamWriter(leaderboardFilePath))
            {
                for (int i = 0; i < lb.Count; i++)
                {
                    var e = lb[i];
                    w.WriteLine($"PlayerName: {e.PlayerName}");
                    w.WriteLine($"Level: {e.Level}");
                    w.WriteLine($"Score: {e.Score}");
                    if (i < lb.Count - 1) w.WriteLine("---");
                }
            }
        }

        static void ShowLeaderboard()
        {
            if (!File.Exists(leaderboardFilePath))
            {
                Console.WriteLine("Нет лидеров");
                return;
            }

            string[] lines = File.ReadAllLines(leaderboardFilePath);

            Console.WriteLine("Место Игрок Уровень Счёт");
            Console.WriteLine("-------------------------");

            int place = 1;
            for (int i = 0; i < lines.Length; i += 4)
            {
                if (i + 2 >= lines.Length) break;

                string name = lines[i].Substring(11).Trim();
                string lvl = lines[i + 1].Substring(6).Trim();
                string score = lines[i + 2].Substring(6).Trim();

                Console.WriteLine($"{place,-5} {name,-10} {lvl,-7} {score,-10}");
                place++;
            }
        }
    }
}
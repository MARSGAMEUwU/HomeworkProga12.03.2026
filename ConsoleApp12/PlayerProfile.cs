using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
internal class PlayerProfile
{
    public string PlayerName { get; }
    public int MaxLevel { get; set; }
    public int Score { get; set; }

    public PlayerProfile(string playerName, int maxLevel = 1, int score = 0)
    {
        PlayerName = playerName;
        MaxLevel = maxLevel;
        Score = score;
    }
}
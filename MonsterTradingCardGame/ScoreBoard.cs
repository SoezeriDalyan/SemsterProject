﻿using System.Text.Json.Serialization;

namespace MonsterTradingCardGame
{
    internal class ScoreBoard
    {
        //Helps to serialize
        [JsonPropertyName("Place")]
        public int Place { get; private set; }
        [JsonPropertyName("Name")]
        public string Name { get; private set; }
        [JsonPropertyName("Elo")]
        public int Elo { get; private set; }

        public ScoreBoard(int place, string name, int elo)
        {
            Place = place;
            Name = name;
            Elo = elo;
        }

        public override string? ToString()
        {
            return "{" + $"Place = {Place}\nName = {Name}\nElo = {Elo}" + "}";
        }
    }
}
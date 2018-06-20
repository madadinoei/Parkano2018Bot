using System;

namespace Parkano2018Bot.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public DateTime MatchDateTime { get; set; }
    }
}
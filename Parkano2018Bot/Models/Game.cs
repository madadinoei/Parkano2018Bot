using System;
using System.ComponentModel.DataAnnotations;

namespace Parkano2018Bot.Models
{
    public class Game
    {
        [Key]
        public Guid Id { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public DateTime MatchDateTime { get; set; }
    }
}
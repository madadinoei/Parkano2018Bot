using System;
using System.ComponentModel.DataAnnotations;

namespace Parkano2018Bot.Models
{
    public class Poll
    {
        [Key]
        public Guid Id { get; set; }
        public int? HomeGoal { get; set; }
        public int? AwayGoal { get; set; }
        public int GameId { get; set; }
        public bool CanEdit { get; set; }
        public DateTime DateTime { get; set; }
        public User User { get; set; }

    }
}

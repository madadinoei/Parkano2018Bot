using System;
using System.ComponentModel.DataAnnotations;

namespace Parkano2018Bot.Models
{
    public class Poll
    {
        public int HomeGoal { get; set; }
        public int AwayGoal { get; set; }
        [Key]
        public Guid GameId { get; set; }
        public bool CanEdit { get; set; }
    }
}

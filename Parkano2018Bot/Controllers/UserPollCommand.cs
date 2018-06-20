using System;

namespace Parkano2018Bot.Controllers
{
    public class UserPollCommand
    {
        public Guid UserId { get; set; }
        public int HomeGoal { get; set; }
        public int AwayGoal { get; set; }
        public Guid GameId { get; set; }
    }
}
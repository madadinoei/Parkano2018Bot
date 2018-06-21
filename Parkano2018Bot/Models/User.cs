using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Parkano2018Bot.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public int TelegramUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public virtual ICollection<Poll> Polls { get; set; }
        public int Score { get; set; }
    }
}

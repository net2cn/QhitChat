using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace QhitChat_Client.Presistent.Database.Models
{
    public class User
    {
        [Key, Required, StringLength(10, MinimumLength = 1)]
        public string Account { get; set; }

        [Required, StringLength(32, MinimumLength = 1)]
        public string Username { get; set; }
    }
}

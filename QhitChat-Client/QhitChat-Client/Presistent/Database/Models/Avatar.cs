using QhitChat_Client.Presistent.Database.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QhitChat_Client.Presistent.Database.Models
{
    public class Avatar
    {
        [Key]
        public string Account { get; set; }

        [ForeignKey("Account")]
        public User User { get; set; }

        [Required, StringLength(10, MinimumLength = 1)]
        public string Path { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QhitChat_Server.Presistent.Database.Models
{
    public class Avatar
    {
        [Key, StringLength(10, MinimumLength = 1)]
        public string Account { get; set; }

        [ForeignKey("Account")]
        public User User { get; set; }

        [Required]
        public string Path { get; set; }
    }
}

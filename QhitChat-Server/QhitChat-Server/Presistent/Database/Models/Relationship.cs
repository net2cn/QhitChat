using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QhitChat_Server.Presistent.Database.Models
{
    public class Relationship
    {
        [Required]
        public string From { get; set; }

        [ForeignKey("From")]
        public User User { get; set; }

        [Required]
        public string To { get; set; }
    }
}

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
        public User FromUser { get; set; }

        [Required]
        public string To { get; set; }

        [ForeignKey("To")]
        public User ToUser { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace QhitChat_Server.Presistent.Database.Models
{
    public class Relationship
    {
        [Key]
        public string From { get; set; }

        [Required]
        public string To { get; set; }
    }
}

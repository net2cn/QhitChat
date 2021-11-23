using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QhitChat_Server.Presistent.Database.Models
{
    public class Messages : Relationship
    {
        public string Content { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public int IsSent { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QhitChat_Client.Presistent.Database.Models
{
    class Messages : Relationship
    {
        public string Content { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public int IsSent { get; set; } // 0 - message not delivered yet; -1 - message delivered.
    }
}

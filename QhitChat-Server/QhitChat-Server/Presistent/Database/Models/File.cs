using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QhitChat_Server.Presistent.Database.Models
{
    public class File
    {
        [Key, StringLength(36, MinimumLength = 36)]
        public string Uuid { get; set; }

        [Required, StringLength(10, MinimumLength = 1)]
        public string From { get; set; }

        [Required, StringLength(255, MinimumLength = 1)]
        public string OriginalName { get; set; }

        [Required]
        public int IsReceived { get; set; } // 0 - Done.

        [Required]
        public DateTime CreatedOn { get; set; }
    }
}

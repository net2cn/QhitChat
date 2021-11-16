using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QhitChat_Server.Presistent.Database.Models
{
    public class Avatar
    {
        [Key, Required, StringLength(10, MinimumLength = 1)]
        public string Account { get; set; }

        [Required, StringLength(10, MinimumLength = 1)]
        public string Path { get; set; }
    }
}

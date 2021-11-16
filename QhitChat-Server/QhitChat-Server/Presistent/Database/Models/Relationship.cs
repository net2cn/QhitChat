﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace QhitChat_Server.Presistent.Database.Models
{
    public class Relationship
    {
        [Key, Required, StringLength(10, MinimumLength = 1)]
        public string From { get; set; }

        [Required, StringLength(10, MinimumLength = 1)]
        public string To { get; set; }
    }
}

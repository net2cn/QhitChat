using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QhitChat_Client.Presistent.Database.Models
{
    public class Relationship
    {
        [Key]
        public string From { get; set; }

        [Required]
        public string To { get; set; }
    }
}

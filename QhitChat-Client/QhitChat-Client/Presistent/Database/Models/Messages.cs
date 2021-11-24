using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace QhitChat_Client.Presistent.Database.Models
{
    [DataContract]
    public class Messages
    {
        [DataMember(Order = 0)]
        [Required]
        public string From { get; set; }

        [DataMember(Order = 1)]
        [Required]
        public string To { get; set; }

        [DataMember(Order = 2)]
        public string Content { get; set; }

        [DataMember(Order = 3)]
        [Required]
        public DateTime CreatedOn { get; set; }

        [DataMember(Order = 4)]
        [Required]
        public int IsSent { get; set; } // 0 - message not delivered yet; -1 - message delivered.

        [DataMember(Order = 5)]
        [Key]
        public ulong Id { get; set; }

        public override string ToString()
        {
            return Content;
        }
    }
}

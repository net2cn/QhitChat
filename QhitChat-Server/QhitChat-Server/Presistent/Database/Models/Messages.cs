using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace QhitChat_Server.Presistent.Database.Models
{
    [DataContract]
    public class Messages
    {
        [DataMember(Order = 0)]
        [Required]
        public string From { get; set; }

        [ForeignKey("From")]
        public User FromUser { get; set; }

        [DataMember(Order = 1)]
        [Required]
        public string To { get; set; }

        [ForeignKey("To")]
        public User ToUser { get; set; }

        [DataMember(Order = 2)]
        public string Content { get; set; }

        [DataMember(Order = 3)]
        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public int IsSent { get; set; } // 0 - message not delivered yet; -1 - message delivered.

        [Key]
        public ulong Id { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MessagePack;
using System.Runtime.Serialization;

namespace QhitChat_Server.Presistent.Database.Models
{
    [DataContract]
    public class Relationship
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
    }
}

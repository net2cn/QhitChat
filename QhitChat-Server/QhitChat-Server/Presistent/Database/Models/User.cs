using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace QhitChat_Server.Presistent.Database.Models
{
    public class User
    {
        [Key]
        public string Uuid { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }     // Password is hashed using SHA-512 before it is sent to server.
        public int Status { get; set; }       // User status.
        public string Token { get; set; }        // User token for session authentication.
    }
}

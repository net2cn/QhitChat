using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace QhitChat_Server.Presistent.Database.Models
{
    public class User
    {
        [Key, Required, StringLength(10, MinimumLength = 1)]
        public string Account { get; set; }

        [Required, StringLength(32, MinimumLength = 1)]
        public string Username { get; set; }

        [Required, StringLength(128, MinimumLength = 128)]
        public string Password { get; set; }     // Password is hashed using SHA-512 before it is sent to server.

        [Required, StringLength(8, MinimumLength = 8)]
        public string Salt { get; set; }

        [Required]
        public int Status { get; set; }       // User status.

        [StringLength(36, MinimumLength = 36)]
        public string Token { get; set; }        // User token for session authentication.
    }
}

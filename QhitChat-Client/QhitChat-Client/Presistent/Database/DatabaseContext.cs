using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QhitChat_Client.Presistent.Database.Models;
using System.Collections.Generic;

namespace QhitChat_Client.Presistent.Database
{
    class DatabaseContext:DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Relationship> Relationship { get; set; }
        public DbSet<Avatar> Avatar { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Construct database connection string
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "QhitChat.db" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
        }
    }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QhitChat_Server.Presistent.Database.Models;
using System.Collections.Generic;

namespace QhitChat_Server.Presistent.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Relationship> Relationship { get; set; }
        public DbSet<Avatar> Avatar { get; set; }
        public DbSet<Messages> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Construct database connection string
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "QhitChat.db" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            List<User> users = new List<User>() {
                            new User { Username = "net2cn", Account = "net2cn", Password = "42d25ef0ea0669f3a13a498f3c868e024a51c8e11594f27fc167c38852b46d302d7afd8818338574e636a1d44d65f410d2812c4e6e95cd6efea00059cc76b131", Salt = "964acf26" },
                            new User { Username = "dancingmon", Account = "dancingmon", Password = "303b6b7d5c4f01e829d730b47c62a025ced1546e4f963b55b487343628b5fc2629ed6bdb0c95b95e57e6bbc366a40cb4b6d2bfa2d6ebb81a946c9b8c34a9b9b5", Salt = "96e4ac07" },
                            new User { Account = "jumpingdog", Username = "jumpingdog我", Password = "572043a548c73f8e576fc9ab49008f8574cb6db48208c9370b0d04624c5cc7ff27d44f247a4e7f6b2fdeb9e6da98222fac5146afa63134fbde874bc581705745", Salt = "dcd8951b" },
                            new User { Account = "noisycat", Username = "noisycat👍", Password = "72c1222d07bc7569f010e28954324a77ac8b878f537a642aafa9f6ff6a3ea54be001fc85ef99a2c610c97a207e1c741aad13def0ad62bfb6d5414458ffe0a7d7", Salt = "65422a02" },
                            new User { Account = "duplicatedone", Username = "duplicatedone🎶", Password = "c2b9e1f0173644e24b357a1aa6621d816b294b979148dd835633ca98eb2e8898565beb7a2bb73f89d462977939688ca063881fc7f38def51d4b49c6a6ba67fea", Salt = "8e60dcae" },
                            new User { Account = "not_avaliable", Username = "not_avaliable💋", Password = "ee4070ac73a6526800b6dc1af4a35abb67e3f689a288f413585bdabd11aff55c566b4e2b2e2048b553ef118eaf5ef9ddad0cdc38321a797cd2bbd4a19a25d4fa", Salt = "a443f8d7" },
                            new User { Account = "0xd4da", Username = "0xd4da✌", Password = "d725b80d1514981d4e6ecb84752b93a66e9f050473aa6952361eef05135399601f72c29a797ad5421e0a3252c4605d33a326bf7ad25249a64e59acb58bfcf254", Salt = "57d586d3" },
                            new User { Account = "DEADBEEF", Username = "DEADBEEF✔", Password = "7535e02eaca30c0820538a5c0eae703d6cc7fba3084a08d96078adcf70ed86aa64f93abfd9a215985b001a5aef0c05f552d111b54c9450112402e15d947b99f9", Salt = "0e4a9b87" }
            };

            modelBuilder.Entity<User>().HasData(users);

            modelBuilder.Entity<Relationship>().HasKey(c => new { c.From, c.To });

            modelBuilder.Entity<Relationship>().HasData(
                new Relationship { From = users[0].Account, To = users[1].Account },
                new Relationship { From = users[1].Account, To = users[0].Account },
                new Relationship { From = users[0].Account, To = users[2].Account },
                new Relationship { From = users[2].Account, To = users[0].Account },
                new Relationship { From = users[0].Account, To = users[3].Account },
                new Relationship { From = users[3].Account, To = users[0].Account },
                new Relationship { From = users[1].Account, To = users[2].Account },
                new Relationship { From = users[2].Account, To = users[1].Account },
                new Relationship { From = users[1].Account, To = users[3].Account },
                new Relationship { From = users[3].Account, To = users[1].Account }
            );

            modelBuilder.Entity<Avatar>().HasData(
                new Avatar { Account = users[0].Account, Path = "./Avatars/d642eccd-5371-4304-af5e-947e476a22a5.png" }
            );

            modelBuilder.Entity<Messages>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
        }
    }
}

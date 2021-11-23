using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QhitChat_Client.Presistent.Database;
using QhitChat_Client.Presistent.Database.Models;
using System;
using System.Collections.Generic;

namespace QhitChat_Client.Presistent
{
    class Presistent
    {
        public static readonly DatabaseContext DatabaseContext = new DatabaseContext();

        private static readonly Lazy<Presistent> lazy =
            new Lazy<Presistent>(() => new Presistent());

        public static Presistent Instance { get { return lazy.Value; } }

        private Presistent()
        {
            // Connect to database.
            DatabaseContext.Database.EnsureCreated();
            DatabaseContext.Database.Migrate();
        }
    }
}

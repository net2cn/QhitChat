using Microsoft.EntityFrameworkCore;
using QhitChat_Server.Presistent.Database;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace QhitChat_Server.Presistent
{
    class Presistent
    {
        public static readonly DatabaseContext DatabaseContext = new DatabaseContext();
        public static readonly Dictionary<string, ObservableCollection<Database.Models.Messages>> MessageQuene = new Dictionary<string, ObservableCollection<Database.Models.Messages>>();

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

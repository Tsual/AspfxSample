using IdentityAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAPI.Data
{
    public class SqliteContext:DbContext
    {
        public DbSet<mUser> sUser { get; set; }
        public DbSet<mValue> sValue { get; set; }

        public const string ConnectUri = "DataSource=sqlite0.db";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectUri);
        }
    }
}

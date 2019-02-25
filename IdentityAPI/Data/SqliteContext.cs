using IdentityAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAPI.Data
{
    public class SqliteContext:DbContext
    {
        private readonly IConfiguration _config;
        public SqliteContext(IConfiguration config)
        {
            _config = config;
        }
        public DbSet<mUser> sUser { get; set; }
        public DbSet<mValue> sValue { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_config["sqlite:connect_string"]);
        }
    }
}

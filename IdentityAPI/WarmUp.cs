using Helper;
using IdentityAPI.Core;
using IdentityAPI.Data;
using IdentityAPI.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityAPI
{
    public class WarmUp
    {
        public static void DoWork(IConfiguration configuration)
        {
            JwtCore.SecretKey = new SymmetricSecurityKey(Byte16String.Decode(configuration["jwt:SecretKey"]));
            SqliteContext _context = new SqliteContext(configuration);
            DbWarmUpAsync(_context).Wait();
            RedisWarmUp(configuration, _context);
        }

        private static async Task DbWarmUpAsync(SqliteContext _context)
        {
            _context.Database.EnsureCreated();
            var value_count = _context.sValue.Count();
            if (value_count < 1000)
            {
                Parallel.For(1000 - value_count, 999, async i =>
                {
                    string key = Byte16String.RandomString(16);
                    while (_context.sValue.Find(key) != null) key = Byte16String.RandomString(16);
                    await _context.sValue.AddAsync(new mValue() { Key = key, Value = Byte16String.RandomString(64) });
                });
                await _context.SaveChangesAsync();
            }
        }

        private static void RedisWarmUp(
            IConfiguration configuration,
            SqliteContext _context
            )
        {
            var redis = Data.RedisCache.Instance[configuration["redis:connect_string"]].GetDatabase();
            var par_result = Parallel.ForEach(_context.sValue, async db_pair =>
               {
                   var pair_key = "VALUE_" + db_pair.Key;
                   var pair_value = await redis.StringGetAsync(pair_key);
                   if (pair_value.IsNullOrEmpty)
                       await redis.StringSetAsync(pair_key, db_pair.Value);
               });

        }
    }
}

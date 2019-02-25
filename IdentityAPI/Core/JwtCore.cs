using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAPI.Core
{
    public class JwtCore
    {
        public static SecurityKey SecretKey { get; set; }

        public static bool Check(IDatabase RedisDatabase, string Audience)
        {
            var redis_pair = RedisDatabase.StringGet("JWT_TOKEN_OVERTIME_" + Audience);
            if (redis_pair.IsNull) return false;
            return DateTime.Parse(redis_pair.ToString()) > DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RedisDatabase"></param>
        /// <param name="Audience"></param>
        /// <param name="Issuer"></param>
        /// <param name="overtime"></param>
        /// <returns>token of the audience</returns>
        public static string Regist(IDatabase RedisDatabase, string Audience, string Issuer, int overtime)
        {
            var token = new JwtSecurityToken(issuer: Issuer, audience: Audience,
                notBefore: DateTime.Now, expires: DateTime.Now.AddMinutes(30),
                signingCredentials: new SigningCredentials(SecretKey, SecurityAlgorithms.HmacSha256));
            RedisDatabase.StringSet("JWT_TOKEN_OVERTIME_" + Audience, DateTime.Now.AddMinutes(overtime).ToString());
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

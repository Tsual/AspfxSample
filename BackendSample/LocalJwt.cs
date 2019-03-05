using BackendSample;
using BackendSample.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSample
{
    public class LocalJwt
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


namespace Microsoft.Extensions.DependencyInjection
{
    public static class LocalJwtExtensions
    {
        public static AuthenticationBuilder AddLocalJwt(this AuthenticationBuilder builder,IConfiguration Configuration)
        {
            return builder.AddJwtBearer(arg =>
            {

                arg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = LocalJwt.SecretKey,

                    ValidateIssuer = true,
                    ValidIssuer = Configuration["jwt:Issuer"],

                    ValidateAudience = true,
                    //ValidAudience = JwtSetting.Audience,

                    // Validate the token expiry
                    ValidateLifetime = true,

                    // If you want to allow a certain amount of clock drift, set that here:
                    ClockSkew = TimeSpan.Zero,

                    AudienceValidator = (aud, key, token) =>
                    {
                        bool res = true;
                        foreach (var aud_t in aud)
                            res &= LocalJwt.Check(RedisCache.Instance[Configuration["redis:connect_string"]].GetDatabase(), aud_t);
                        return res;
                    },

                    //IssuerValidator = (iss, key, token) =>
                    //  {
                    //      return "";
                    //  },
                };
            });
        }
    }
}
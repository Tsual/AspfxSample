using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentitySeverExtension
    {
        //dotnet add package IdentityModel
        public static AuthenticationBuilder AddIdentityServerJwt(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            return builder.AddJwtBearer("Bearer", options =>
            {
                options.Authority = configuration["identity_server_4:url"];
                options.RequireHttpsMetadata = false;

                options.Audience = configuration["identity_server_4:jwt_api"];
            });
        }
    }
}

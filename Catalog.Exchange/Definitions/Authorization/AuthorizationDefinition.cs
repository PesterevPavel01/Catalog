using System.Text;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Entities.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Catalog.Exchange.Definitions.Authorization
{
    public class AuthorizationDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            var authSettings = builder.Configuration.GetSection("Authorization").Get<AuthorizationSettings>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrator", policy =>
                    policy.RequireRole("Administrator"));
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(config =>
            {
                byte[] secretBytes = Encoding.UTF8.GetBytes(authSettings.SecretKey);

                var key = new SymmetricSecurityKey(secretBytes);

                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authSettings.Issuer,
                    ValidAudience = authSettings.Audience,
                    IssuerSigningKey = key,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };

                config.Authority = authSettings.Authority;
                config.RequireHttpsMetadata = false;
            });
        }
    }
}

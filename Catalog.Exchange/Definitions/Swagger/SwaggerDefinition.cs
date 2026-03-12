using System.Reflection;
using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Microsoft.OpenApi.Models;

namespace Catalog.ExchangeService.Definitions.Swagger
{
    public class SwaggerDefinition : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
        {
            //if (!app.Environment.IsDevelopment())
            //{
            //    return;
            //}

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1.0");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "Api v2.0");
                c.SwaggerEndpoint("/swagger/v3/swagger.json", "Api v3.0");

                // Настройка OAuth
                c.OAuthClientId("swagger-ui");
                c.OAuthClientSecret("swagger-ui-secret");
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });
        }

        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("x-api-version"),
                    new UrlSegmentApiVersionReader());
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Version = "v1",
                    Title = "Сервис Catalog.ExchangeServiсe",
                    Description = "Версия 1.0",
                });

                options.SwaggerDoc("v2", new OpenApiInfo()
                {
                    Version = "v2",
                    Title = "Сервис Catalog.ExchangeServiсe",
                    Description = "Версия 2.0",
                });

                options.SwaggerDoc("v3", new OpenApiInfo()
                {
                    Version = "v3",
                    Title = "Сервис Catalog.ExchangeServiсe",
                    Description = "Версия 3.0",
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Введите валидный токен",
                    Name = "Авторизация",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme()
                            {
                                Reference = new OpenApiReference()
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                            },
                            Array.Empty<string>()
                        }
                    }
                );

                var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
            });


        }
    }
}
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Elasticsearch.NotSearchCaseSensitive.API.Filters.Swaggers;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Elasticsearch.NotSearchCaseSensitive.API.Extensions
{
    public static class SwaggerExtensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            // Registra o Swagger para documentar a API
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "TeamWork Plus",
                    Description = "Portal para testes",
                    TermsOfService = "None",
                });

                // Swagger 2.+ support
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(security);

                c.DocumentFilter<HideInSwaggerFilter>();

                // Adiciona os comentários da API no Swagger JSON da UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);


                // Linha adicionada para resolver seguinte erro
                // Conflicting schemaIds: Duplicate schemaIds
                // Acontece devido os command's terem o mesmo nome
                c.CustomSchemaIds(x => x.FullName);
            });
        }

        public static void ConfigSwagger(this IApplicationBuilder app)
        {
            // Habilita o Middleware do Swagger.
            app.UseSwagger();

            //Configura o Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = string.Empty;

                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
                c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "API V1");
            });
        }

        public static IMvcBuilder AddSwaggerMediaTypes(this IMvcBuilder builder)
        {
            builder.AddMvcOptions(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            });

            return builder;
        }
    }
}
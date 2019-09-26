using FluentValidation.AspNetCore;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using Elasticsearch.NotSearchCaseSensitive.API.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Elasticsearch.NotSearchCaseSensitive.API.Extensions
{
    public static class MvcExtensions
    {
        public static void AddFilters(this IServiceCollection services)
        {
            services.AddMvc(options => options.Filters.Add(new ExceptionHandlerAttribute()));
        }

        public static void UseOData(this IApplicationBuilder app)
        {
            // Habilitando o ODATA
            app.UseMvc(routebuilder =>
            {
                routebuilder.EnableDependencyInjection(builder =>
                {
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(ODataUriResolver), sp => new StringAsEnumResolver() { EnableCaseInsensitive = true });
                });
                routebuilder
                    .Select()
                    .Filter()
                    .OrderBy()
                    .MaxTop(int.MaxValue)
                    .Count();
            });
        }

        public static void AddMVC(this IServiceCollection services)
        {
            services.AddMvc()
                    .AddSwaggerMediaTypes()
                    .AddFluentValidation(fvc => fvc.RegisterValidatorsFromAssemblyContaining<Application.AppModule>())
                    .AddJsonOptions(opt =>
                    {
                        opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                        opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    });
        }
    }
}

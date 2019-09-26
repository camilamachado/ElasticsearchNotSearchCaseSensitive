using Microsoft.Extensions.DependencyInjection;
using Elasticsearch.NotSearchCaseSensitive.Infra.Extensions;

namespace Elasticsearch.NotSearchCaseSensitive.API.Extensions
{
    public static class AutoMapperExtensions
    {
        public static void AddAutoMapper(this IServiceCollection services)
        {
            services.ConfigureProfiles(typeof(Startup), typeof(Application.AppModule));
        }
    }
}

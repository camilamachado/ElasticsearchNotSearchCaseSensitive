using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Elasticsearch.Net;
using Elasticsearch.NotSearchCaseSensitive.Infra.Settings.Entities;
using Elasticsearch.NotSearchCaseSensitive.Infra.Settings;
using Nest.JsonNetSerializer;

namespace Elasticsearch.NotSearchCaseSensitive.API.Extensions
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elasticsearchSettings = configuration.LoadSettings<ElasticsearchSettings>("ElasticsearchSettings");

            var uri = new Uri(elasticsearchSettings.Uri);
            var index = elasticsearchSettings.Index;

            var pool = new SingleNodeConnectionPool(uri);
            var connectionSettings = new ConnectionSettings(pool, JsonNetSerializer.Default).DefaultIndex(index);

            var client = new ElasticClient(connectionSettings);

            services.Add(ServiceDescriptor.Singleton<IElasticClient>(client));
        }
    }
}
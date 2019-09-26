using Nest;

namespace Elasticsearch.NotSearchCaseSensitive.Infra.Data.Contexts
{
    public class ElasticsearchLogDbContext
    {
        public IElasticClient Database { get; private set; }

        public ElasticsearchLogDbContext(IElasticClient database)
        {
            Database = database;
        }
    }
}

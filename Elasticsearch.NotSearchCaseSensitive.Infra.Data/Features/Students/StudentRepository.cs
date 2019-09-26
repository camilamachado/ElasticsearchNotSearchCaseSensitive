using Elasticsearch.Net;
using Elasticsearch.NotSearchCaseSensitive.Domain.Features.Students;
using Elasticsearch.NotSearchCaseSensitive.Infra.Data.Contexts;
using Elasticsearch.NotSearchCaseSensitive.Infra.Structs;
using System;
using System.Threading.Tasks;

namespace Elasticsearch.NotSearchCaseSensitive.Infra.Data.Features.Students
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ElasticsearchLogDbContext _context;

        public StudentRepository(ElasticsearchLogDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Exception, Unit>> AddAsync(Student student)
        {
            var indexResponse = await _context.Database.IndexDocumentAsync(student);

            if (!indexResponse.IsValid)
            {
                return new ElasticsearchClientException(indexResponse.DebugInformation);
            }

            return Unit.Successful;
        }
    }
}

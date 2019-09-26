using Elasticsearch.NotSearchCaseSensitive.Infra.Structs;
using System;
using System.Threading.Tasks;

namespace Elasticsearch.NotSearchCaseSensitive.Domain.Features.Students
{
    public interface IStudentRepository
    {
        Task<Result<Exception, Unit>> AddAsync(Student student);
    }
}

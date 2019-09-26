using Elasticsearch.NotSearchCaseSensitive.Infra.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elasticsearch.NotSearchCaseSensitive.Infra.Extensions
{
    public static class EnumerableExtensions
    {
        public static Result<Exception, IQueryable<TSuccess>> AsResult<TSuccess>(this IEnumerable<TSuccess> source)
        {
            return Result.Run(() => source.AsQueryable());
        }
    }
}

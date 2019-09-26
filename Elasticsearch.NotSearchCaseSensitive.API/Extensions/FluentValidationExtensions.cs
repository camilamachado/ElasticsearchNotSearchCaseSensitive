using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using System.Reflection;

namespace Elasticsearch.NotSearchCaseSensitive.API.Extensions
{
    public static class FluentValidationExtensions
    {
        public static void AddValidators(this IServiceCollection services, Container container)
        {
            container.Collection.Register(typeof(IValidator<>), typeof(Application.AppModule).GetTypeInfo().Assembly);
        }
    }
}

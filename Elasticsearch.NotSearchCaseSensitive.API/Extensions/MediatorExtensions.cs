using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Elasticsearch.NotSearchCaseSensitive.API.Behaviours;
using Elasticsearch.NotSearchCaseSensitive.Application;
using SimpleInjector;
using System.Collections.Generic;
using System.Reflection;

namespace Elasticsearch.NotSearchCaseSensitive.API.Extensions
{
    public static class MediatorExtensions
    {
        public static void AddMediator(this IServiceCollection services, Container container)
        {
            var assembly = GetAssemblies();

            container.RegisterSingleton<IMediator, Mediator>();
            container.Register(() => new ServiceFactory(container.GetInstance), Lifestyle.Singleton);
            container.Register(typeof(IRequestHandler<,>), assembly);
            container.Register(typeof(IRequestHandler<>), assembly);

            var notificationHandlerTypes = container.GetTypesToRegister(typeof(INotificationHandler<>), assembly);
            container.Collection.Register(typeof(INotificationHandler<>), notificationHandlerTypes);

            container.Collection.Register(typeof(IPipelineBehavior<,>), new[]
            {
                typeof(ValidationPipeline<,>)
            });
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            yield return typeof(IMediator).GetTypeInfo().Assembly;
            yield return typeof(AppModule).GetTypeInfo().Assembly;
        }
    }
}

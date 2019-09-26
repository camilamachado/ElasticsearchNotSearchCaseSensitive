using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Elasticsearch.NotSearchCaseSensitive.Infra.Settings;
using Elasticsearch.NotSearchCaseSensitive.Infra.Settings.Entities;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;
using Elasticsearch.NotSearchCaseSensitive.Infra.Data.Features.Students;
using Elasticsearch.NotSearchCaseSensitive.Domain.Features.Students;
using Elasticsearch.NotSearchCaseSensitive.Infra.Data.Contexts;

namespace Elasticsearch.NotSearchCaseSensitive.API.Extensions
{
    public static class SimpleInjectorExtensions
    {
        public static void AddSimpleInjector(this IServiceCollection services, Container container)
        {
            // Define que a instância vai existir dentro de um determinado escopo (implícito ou explícito).
            // Assume o fluxo de controle dos métodos assíncronos automaticamente
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(container));

            // Ao invocar o método de extensão UseSimpleInjectorAspNetRequestScoping, o tempo que uma 
            // requisição está ativa é o período que um escopo vai estar ativo. 
            services.UseSimpleInjectorAspNetRequestScoping(container);

            services.EnableSimpleInjectorCrossWiring(container);
        }

        public static void AddDependencies(this IServiceCollection services,
            Container container,
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment)
        {
            RegisterFeatures(container);

            // Adicionando o IHttpClientFactory para utilizar o HttpClient
            services.AddScoped(s => s.GetRequiredService<IHttpClientFactory>().CreateClient());

            container.Register<ElasticsearchLogDbContext>(Lifestyle.Scoped);
        }

        private static void RegisterFeatures(Container container)
        {
            container.Register<IStudentRepository, StudentRepository>();
        }

    }
}

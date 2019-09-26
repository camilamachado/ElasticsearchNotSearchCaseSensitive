using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Elasticsearch.NotSearchCaseSensitive.API.Extensions;
using SimpleInjector;

namespace Elasticsearch.NotSearchCaseSensitive.API
{
    public class Startup
    {
        public static Container Container = new Container();

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();
            services.AddOData();
            services.AddSimpleInjector(Container);
            services.AddDependencies(Container, Configuration, HostingEnvironment);
            services.AddMediator(Container);
            services.AddValidators(Container);
            services.AddFilters();
            services.AddSwagger();
            services.AddElasticsearch(Configuration);
            services.AddMVC();

            services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Container.AutoCrossWireAspNetComponents(app);

            Container.RegisterMvcControllers(app);

            app.ConfigSwagger();

            app.UseOData();
        }
    }
}

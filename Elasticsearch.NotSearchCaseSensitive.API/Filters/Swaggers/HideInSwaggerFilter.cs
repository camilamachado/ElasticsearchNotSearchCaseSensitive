using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Elasticsearch.NotSearchCaseSensitive.API.Filters.Swaggers
{
    /// <summary>
    /// Quando tentamos utilizar o Swagger no APS.NET Core com OData se deparamos com esse erro:
    /// https://stackoverflow.com/questions/50940593/integrate-swashbuckle-swagger-with-odata-in-asp-net-core
    /// A solução acima funciona, porem por algum motivo ele não recohece os parametros do ODATA, expondo várias models desnecessárias na documentação.
    /// Issue: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/807
    /// Até que implementem essa funcionalide criamos esse filtro para anular os métodos OData no Swagger e gerenciar essas models.
    /// </summary>
    public class HideInSwaggerFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var contextApiDescription in context.ApiDescriptions)
            {
                var actionDescriptor = (ControllerActionDescriptor)contextApiDescription.ActionDescriptor;

                if (actionDescriptor.MethodInfo.GetParameters()
                    .Any(p => p.ParameterType.Namespace.Equals("Microsoft.AspNet.OData.Query")))
                {
                    var key = "/" + contextApiDescription.RelativePath.TrimEnd('/');

                    var pathItem = swaggerDoc.Paths[key];
                    if (pathItem == null)
                        continue;

                    switch (contextApiDescription.HttpMethod.ToUpper())
                    {
                        case "GET":
                            pathItem.Get = null;
                            break;

                        case "POST":
                            pathItem.Post = null;
                            break;

                        case "PUT":
                            pathItem.Put = null;
                            break;

                        case "DELETE":
                            pathItem.Delete = null;
                            break;
                    }

                    if (pathItem.Get == null
                        && pathItem.Post == null
                        && pathItem.Put == null
                        && pathItem.Delete == null)
                    {
                        RemovePath(swaggerDoc, key);
                        ManageModels(swaggerDoc);
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Método responsável por remover os path's que utulizam o OData e os models desnecessários.
        /// </summary>
        /// <param name="swaggerDoc">Utilizado para acessar as definições.</param>
        /// <param name="key">Contem o path que será removido. </param>
        private void RemovePath(SwaggerDocument swaggerDoc, string key)
        {
            swaggerDoc.Paths.Remove(key);
        }

        /// <summary>
        /// Este método é responsável por deletar os Models desnecessários
        /// </summary>
        /// <param name="swaggerDoc">Todos os models estão nas "Definitions" do SwaggerDoc</param>
        private static void ManageModels(SwaggerDocument swaggerDoc)
        {
            var models = swaggerDoc.Definitions
                .Where(
                   c => c.Key.Contains("Command")
                || c.Key.Contains("ViewModel")
                || c.Key.Contains("Payload"))
                .ToList();

            swaggerDoc.Definitions.Clear();

            foreach (var model in models)
            {
                swaggerDoc.Definitions.Add(model);
            }

            #endregion Private Methods
        }
    }
}
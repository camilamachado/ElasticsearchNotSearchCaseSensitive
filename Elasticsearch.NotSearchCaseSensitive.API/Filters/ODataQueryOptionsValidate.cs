﻿using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Elasticsearch.NotSearchCaseSensitive.API.Filters
{
    /// <summary>
    /// Atributo de uso [ODataQueryOptionsValidate] para validar as opções de odata nos controllers (ODataQueryOptions).
    ///
    /// Nesse atributo é configurado o que pode ou não ser executado no odata.
    /// Por padrão, desativa a opção de $expand.
    /// </summary>
    public class ODataQueryOptionsValidateAttribute : ActionFilterAttribute
    {
        private ODataValidationSettings oDataValidationSettings;

        /// <summary>
        ///  No construtor, cria uma nova configuração de validação para odata
        /// </summary>
        public ODataQueryOptionsValidateAttribute(AllowedQueryOptions allowedQueryOptions = AllowedQueryOptions.All ^ AllowedQueryOptions.Expand)
        {
            oDataValidationSettings = new ODataValidationSettings() { AllowedQueryOptions = allowedQueryOptions };
        }

        /// <summary>
        /// Ao receber uma chamada http no controller, é invocado esse método para validar as opções do odata
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.ActionArguments.Any(a => a.Value != null && a.Value.GetType().Name.Contains(typeof(ODataQueryOptions).Name)))
            {
                var odataQueryOptions = (ODataQueryOptions)actionContext.ActionArguments.Where(a => a.Value != null && a.Value.GetType().Name.Contains(typeof(ODataQueryOptions).Name)).FirstOrDefault().Value;
                odataQueryOptions.Validate(oDataValidationSettings);
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
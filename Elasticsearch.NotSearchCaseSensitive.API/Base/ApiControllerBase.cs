using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elasticsearch.NotSearchCaseSensitive.API.Exceptions;
using Elasticsearch.NotSearchCaseSensitive.API.Extensions;
using Elasticsearch.NotSearchCaseSensitive.Domain.Exceptions;
using Elasticsearch.NotSearchCaseSensitive.Infra.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.NotSearchCaseSensitive.Infra.Structs;

namespace Elasticsearch.NotSearchCaseSensitive.API.Base
{
    [ApiController]
    [Authorize]
    public class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Retorna o Id do usário que está fazendo a chamada.
        /// </summary>
        protected int UserId
        {
            get { return Convert.ToInt32(GetClaimValue("UserId")); }
        }

        /// <summary>
        /// Retorna o cliente do usuário, ser houver.
        /// </summary>
        protected string CustomerName
        {
            get { return string.IsNullOrWhiteSpace(GetClaimValue("CustomerName")) ? string.Empty : GetClaimValue("CustomerName"); }
        }

        #region Handlers    

        /// <summary>
        /// Manuseia o result. Valida se é necessário retornar erro ou o próprio TSuccess
        /// </summary> 
        /// <typeparam name="TFailure"></typeparam>
        /// <typeparam name="TSuccess"></typeparam>
        /// <param name="result">Objeto Result utilizado nas chamadas.</param>
        /// <returns></returns>
        protected IActionResult HandleCommand<TFailure, TSuccess>
            (Result<TFailure, TSuccess> result) where TFailure : Exception
        {
            return result.IsFailure ? HandleFailure(result.Failure) : Ok(result.Success);
        }

        /// <summary>
        /// Aplica o filtro (odata) a query e retora um pageresult criado através do project da TQueryOptions para TResult. 
        /// </summary>
        /// <typeparam name="TQueryOptions">Tipo do obj de origem (domínio)</typeparam>
        /// <typeparam name="TResult">Tipo de retorno objQuery</typeparam>
        /// <param name="query">IQueryable(TQueryOptions)</param>
        /// <param name="queryOptions">OdataQueryOptions</param>
        /// <returns>PageResult(TResult)</returns>
        //protected async Task<PageResult<TResult>> HandlePageResult<TQueryOptions, TResult>
        //    (IQueryable<TQueryOptions> query, ODataQueryOptions<TQueryOptions> queryOptions)
        //{
        //    var queryResults = queryOptions.ApplyTo(query);
        //    var list = await queryResults.ProjectToListAsync<TResult>();
        //    var pageResult = new PageResult<TResult>(list,
        //                                            Request.HttpContext.ODataFeature().NextLink,
        //                                            Request.HttpContext.ODataFeature().TotalCount);
        //    return pageResult;
        //}

        /// <summary>
        /// Verifica a exceção passada por parametro para passar o StatusCode correto para o frontend.
        /// </summary>
        /// <typeparam name="T">Qualquer classe que herde de Exeption</typeparam>
        /// <param name="exceptionToHandle">obj de exceção</param>
        /// <returns></returns>
        protected IActionResult HandleFailure<T>(T exceptionToHandle) where T : Exception
        {
            if (exceptionToHandle is ValidationException)
                return StatusCode(HttpStatusCode.BadRequest.GetHashCode(), (exceptionToHandle as ValidationException).Errors);

            var exceptionPayload = ExceptionPayload.New(exceptionToHandle);
            return exceptionToHandle is BusinessException ?
                StatusCode(HttpStatusCode.BadRequest.GetHashCode(), exceptionPayload) :
                StatusCode(HttpStatusCode.InternalServerError.GetHashCode(), exceptionPayload);
        }

        /// <summary>
        /// Retorna IHttpStatusCode de erro + erros da validação.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validationFailure">Erros de validação (ValidationFailure)</param>
        /// <returns>IActionResult com os erros e status code padrão</returns>
        protected IActionResult HandleValidationFailure<T>(IList<T> validationFailure) where T : ValidationFailure
        {
            return StatusCode(HttpStatusCode.BadRequest.GetHashCode(), validationFailure);
        }

        /// <summary>
        /// Manuseia a query para aplicar as opções do odata, de forma sincrona, para os IQueryables que não vem do banco de dados.
        ///
        /// Esse método vai gerar o PageResult associando os dados (query) com as opções do odata (queryOptions) 
        /// após isso ele monta a resposta HTTP solicitada, conforme headers.
        /// 
        /// ESTE MÉTODO DEVE SER UTILIZADO APENAS PARA DADOS QUE ESTÃO EM MEMÓRIA.
        /// 
        /// </summary>
        /// <typeparam name="TQueryOptions">Tipo do obj de origem (domínio)</typeparam>
        /// <typeparam name="TResult">Tipo de retorno </typeparam>
        /// <param name="result">Objeto Result retornado pelas chamadas.(TQueryOptions)</param>
        /// <param name="queryOptions">OdataQueryOptions(TQueryOptions)</param>
        /// <param name="enableCaseInsensitive">Booleano que informa se é necessário ou não habilitar o case insensitive para o QueryOptions</param>
        /// <returns>IActionResult(TResult) com o resultado da operação</returns>
        protected IActionResult HandleQueryableInMemory<TQueryOptions, TResult>(
                Result<Exception, IQueryable<TQueryOptions>> result,
                ODataQueryOptions<TQueryOptions> queryOptions,
                bool enableCaseInsensitive = false)
        {
            if (result.IsFailure)
                return HandleFailure(result.Failure);

            if (enableCaseInsensitive)
                queryOptions = queryOptions.EnableCaseInsensitive();

            var queryResults = queryOptions.ApplyTo(result.Success);
            var list = queryResults.ProjectTo<TResult>().ToList();

            return Ok(new PageResult<TResult>(list, Request.HttpContext.ODataFeature().NextLink, Request.HttpContext.ODataFeature().TotalCount));
        }

        #endregion

        #region Utils

        /// <summary>
        /// Método responsável por ler do token, no contexto da requisição, as claims codificadas.
        /// </summary>
        /// <param name="type">É o nome da claim (atributo) desejada para leitura no token</param>
        /// <returns>O valor correspondente a claim</returns>
        private string GetClaimValue(string type)
        {
            return ((ClaimsIdentity)HttpContext.User.Identity).FindFirst(type)?.Value;
        }

        /// <summary>
        /// Manuseia o result. Verifica se a resposta é uma falha ou sucesso, retornando os dados apropriados. 
        /// É importante destacar que este método realiza o mapeamento da classe TSource em TResult
        /// </summary> 
        /// <typeparam name="TSource">Classe de origem (ex.: domínio)</typeparam>
        /// <typeparam name="TResult">ViewModel</typeparam>
        /// <param name="result">Objeto Result retornado pelas chamadas.</param>
        /// <returns>Resposta apropriada baseado no result enviado como parâmetro</returns>
        protected IActionResult HandleQuery<TSource, TResult>(Result<Exception, TSource> result)
        {
            return result.IsSuccess ? Ok(Mapper.Map<TSource, TResult>(result.Success)) : HandleFailure(result.Failure);
        }

        #endregion
    }
}
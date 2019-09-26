using Elasticsearch.NotSearchCaseSensitive.Application.Features.Students;
using Elasticsearch.NotSearchCaseSensitive.API.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Elasticsearch.NotSearchCaseSensitive.API.Features.Students
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public StudentController(IMediator mediator) : base()
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Adiciona um estudante
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PostStudentAsync([FromBody] StudentCreate.Command command)
        {
            var result = await _mediator.Send(command);

            return HandleCommand(result);
        }
    }
}

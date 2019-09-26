using MediatR;
using System;
using FluentValidation;
using FluentValidation.Results;
using System.Threading.Tasks;
using System.Threading;
using AutoMapper;
using Elasticsearch.NotSearchCaseSensitive.Domain.Features.Students;
using Elasticsearch.NotSearchCaseSensitive.Infra.Structs;
using Unit = Elasticsearch.NotSearchCaseSensitive.Infra.Structs.Unit;

namespace Elasticsearch.NotSearchCaseSensitive.Application.Features.Students
{
    public class StudentCreate
    {
        public class Command : IRequest<Result<Exception, Unit>>
        {
            public Guid MatriculationNumber { get; set; }
            public string Name { get; set; }

            public ValidationResult Validate()
            {
                return new Validator().Validate(this);
            }

            private class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(st => st.MatriculationNumber).NotEmpty();
                    RuleForEach(st => st.Name).NotEmpty();
                }
            }
        }

        public class Handler : IRequestHandler<Command, Result<Exception, Unit>>
        {
            private readonly IStudentRepository _studentRepository;

            public Handler(IStudentRepository studentRepository)
            {
                _studentRepository = studentRepository;
            }

            public async Task<Result<Exception, Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var student = Mapper.Map<Command, Student>(request);

                var studentCallback = await _studentRepository.AddAsync(student);

                if (studentCallback.IsFailure)
                    return studentCallback.Failure;

                return Unit.Successful;
            }
        }
    }
}

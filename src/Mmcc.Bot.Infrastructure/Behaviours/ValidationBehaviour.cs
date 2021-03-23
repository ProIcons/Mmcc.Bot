using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Behaviours
{
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse>>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Result<TResponse>> next)
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = _validators
                .Select(x => x.Validate(context))
                .SelectMany(x => x.Errors)
                .Where(x => x is not null)
                .ToList();

            if (failures.Any())
            {
                return Task.FromResult(Result<TResponse>.FromError(new GenericError("hhh")));
            }

            return next();
        }
    }
}
using FluentValidation;
using MediatR;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries
{
    public class TestQuery
    {
        public record Query(string? TestString) : IRequest<Result<string>>;

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(x => x.TestString)
                    .NotEmpty();
            }
        }
        
        public class Handler : RequestHandler<Query, Result<string>>
        {
            protected override Result<string> Handle(Query request)
            {
                return request.TestString!;
            }
        }
    }
}
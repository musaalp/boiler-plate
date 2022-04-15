using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sdk.Api.Mediatr.PipelinesBehaviours
{
    public class RequestValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validator;

        public RequestValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validator = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validator.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults.SelectMany(vr => vr.Errors).Where(failure => failure != null);

            if (failures.Count() > 0)
                throw new ValidationException(failures);

            return await next();
        }
    }
}

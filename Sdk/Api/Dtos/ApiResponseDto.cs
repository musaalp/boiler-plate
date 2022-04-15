using FluentValidation.Results;
using System.Collections.Generic;

namespace Sdk.Api.Dtos
{
    public class ApiResponseDto<T>
    {
        public T Data { get; set; }
        public List<ValidationFailure> Errors { get; set; }
    }

    public class ApiResponseDto : ApiResponseDto<object>
    {

    }
}

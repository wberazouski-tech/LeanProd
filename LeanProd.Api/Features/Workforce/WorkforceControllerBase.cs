using LeanProd.Application.Features.Workforce;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.Workforce;

public abstract class WorkforceControllerBase : ControllerBase
{
    protected ActionResult<T> Map<T>(WorkforceResult<T> result)
    {
        if (result.Succeeded) return Ok(result.Value);
        var problem = new ProblemDetails
        {
            Status = result.Error switch
            {
                WorkforceError.NotFound => StatusCodes.Status404NotFound,
                WorkforceError.Validation => StatusCodes.Status400BadRequest,
                WorkforceError.Conflict => StatusCodes.Status409Conflict,
                WorkforceError.Dependency => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            },
            Title = result.Error.ToString(),
            Detail = result.Message,
            Instance = HttpContext.Request.Path
        };
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return StatusCode(problem.Status.Value, problem);
    }
}

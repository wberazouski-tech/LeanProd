using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Application.Features.Scheduling;
using Microsoft.AspNetCore.Mvc;
namespace LeanProd.Api.Features.Scheduling;
public abstract class SchedulingControllerBase : ControllerBase
{
    protected ActionResult<T> Map<T>(SchedulingResult<T> result)
    {
        if (result.Succeeded) return Ok(result.Value);
        var status = result.Error switch { SchedulingError.NotFound => 404, SchedulingError.Validation => 400, SchedulingError.Dependency => 409, _ => 409 };
        return Problem(statusCode: status, title: result.Error.ToString(), detail: result.Message);
    }
}


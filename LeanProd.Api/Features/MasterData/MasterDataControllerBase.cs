using LeanProd.Application.Features.MasterData;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.MasterData;

public abstract class MasterDataControllerBase : ControllerBase
{
    protected ActionResult<T> Map<T>(MasterDataResult<T> result)
    {
        if (result.Succeeded) return Ok(result.Value);
        var status = result.Error switch
        {
            MasterDataError.NotFound => 404,
            MasterDataError.Validation => 400,
            MasterDataError.Dependency => 409,
            _ => 409
        };
        return Problem(statusCode: status, title: result.Error.ToString(), detail: result.Message);
    }
}

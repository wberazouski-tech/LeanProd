using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Common.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase;

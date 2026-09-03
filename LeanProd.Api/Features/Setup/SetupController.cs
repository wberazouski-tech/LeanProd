using System;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Application.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LeanProd.Api.Features.Setup;

[ApiController]
[Route("api/setup")]
public sealed class SetupController(
    IDatabaseProvisioningService provisioning,
    DatabaseStartupState startupState) : ControllerBase
{
    [HttpGet("status")]
    [AllowAnonymous]
    public ActionResult<DatabaseSetupStatus> GetStatus() => Ok(provisioning.GetStatus());

    [HttpPost("test-admin-connection")]
    [AllowAnonymous]
    public async Task<IActionResult> TestAdminConnection(
        DatabaseAdminConnectionRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsSetupAllowed()) return Forbid();
        return await Run(async () =>
        {
            await provisioning.TestAdminConnectionAsync(request, cancellationToken);
            return NoContent();
        });
    }

    [HttpPost("connect-existing")]
    [AllowAnonymous]
    public async Task<ActionResult<DatabaseSetupResult>> ConnectExisting(
        ConnectExistingDatabaseRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsSetupAllowed()) return Forbid();
        return await Run<DatabaseSetupResult>(async () =>
        {
            var result = await provisioning.ConnectExistingAsync(request, cancellationToken);
            return result.Status switch
            {
                DatabaseSetupStatuses.RequiresMigration => Conflict(result),
                DatabaseSetupStatuses.NotLeanProd => BadRequest(result),
                _ => Ok(result)
            };
        });
    }

    [HttpPost("create-new")]
    [AllowAnonymous]
    public async Task<ActionResult<DatabaseSetupResult>> CreateNew(
        CreateDatabaseRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsSetupAllowed()) return Forbid();
        return await Run<DatabaseSetupResult>(async () =>
            Ok(await provisioning.CreateNewAsync(request, cancellationToken)));
    }

    private bool IsSetupAllowed()
    {
        var status = provisioning.GetStatus();
        if (!status.IsConfigured || startupState.DatabaseInitializationFailed)
            return true;

        return User.Identity?.IsAuthenticated == true && User.IsInRole(RoleNames.SystemAdministrator);
    }

    private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (SqlException exception)
        {
            return BadRequest(new { message = MapSqlError(exception) });
        }
    }

    private async Task<ActionResult<T>> Run<T>(Func<Task<ActionResult<T>>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (SqlException exception)
        {
            return BadRequest(new { message = MapSqlError(exception) });
        }
    }

    private static string MapSqlError(SqlException exception)
    {
        if (exception.Number == 18456)
            return "SQL Server rejected access for this SQL login. Check that the login is enabled, SQL authentication is allowed, and the credentials are correct.";
        if (exception.Number == 15151)
            return "The SQL administrator account does not have permission to change this login or database.";
        if (exception.Number is 53 or 11001 or 10060)
            return "SQL Server is unavailable. Check the server name, network and SQL Server service.";
        if (exception.Number == 4060)
            return "Cannot open the selected database.";
        if (exception.Number == 229)
            return "The SQL administrator account does not have enough permissions for this operation.";

        return "SQL Server rejected the operation. Check the connection settings and administrator permissions.";
    }
}

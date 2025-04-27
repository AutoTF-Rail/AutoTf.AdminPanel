using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AutoTf.AdminPanel.Controllers;

public class AuthentikController : ControllerBase, IActionFilter
{
	protected string Username { get; private set; } = null!;
	
	public void OnActionExecuting(ActionExecutingContext context)
	{
		IHeaderDictionary? headers = context.HttpContext.Request.Headers;

		if (!IsAllowedDevice(headers, out string? deviceName))
		{
			context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
			context.Result = new UnauthorizedResult();
		}
		else
			Username = deviceName!;
	}

	public void OnActionExecuted(ActionExecutedContext context) { }
	
	private static bool IsAllowedDevice(IHeaderDictionary headers, out string? deviceName)
	{
		deviceName = "debugPlaceholder";
            
		try
		{
#if DEBUG
			return true;
#endif
			// ReSharper disable once HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected
			deviceName = headers["X-Authentik-Username"].ToString();
                
			// We don't need to further validate this, because all incoming traffic is being routed through authentik anyways, so this is secure enough.
			return !string.IsNullOrEmpty(deviceName);
#pragma warning restore CS0162 // Unreachable code detected
		}
		catch
		{
			return false;
		}
	}
}
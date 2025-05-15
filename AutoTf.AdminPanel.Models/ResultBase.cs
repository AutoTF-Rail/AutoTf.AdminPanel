using System.Net;
using AutoTf.AdminPanel.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AutoTf.AdminPanel.Models;

public abstract class ResultBase : IConvertToActionResult
{
    protected ResultBase(string error)
    {
        Error = error;
    }

    public string Error { get; }
    public ResultCode ResultCode { get; protected init; }
    public bool IsSuccess => ResultCode == ResultCode.Success;
    
    public virtual IActionResult Convert()
    {
        return ResultCode switch
        {
            ResultCode.Success => new OkResult(),
            ResultCode.NotFound => new NotFoundObjectResult(new { error = Error }),
            ResultCode.Unauthorized => new UnauthorizedResult(), // TODO: Fix and add error message
            ResultCode.ValidationError => new BadRequestObjectResult(new { error = Error }),
            ResultCode.BadRequest => new BadRequestObjectResult(new { error = Error }),
            ResultCode.InternalServerError => new ObjectResult(new { error = Error }) { StatusCode = 500 },
            _ => new BadRequestObjectResult(new { error = Error }) 
        };
    }
    
    public static ResultCode MapStatusToResultCode(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.NotFound => ResultCode.NotFound,
        HttpStatusCode.Unauthorized => ResultCode.Unauthorized,
        HttpStatusCode.BadRequest => ResultCode.BadRequest,
        _ => ResultCode.InternalServerError
    };
}
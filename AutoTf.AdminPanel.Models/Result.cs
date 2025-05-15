using AutoTf.AdminPanel.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Models;

public class Result<T> : ResultBase
{
    public T? Value { get; }
    
    private Result(ResultCode resultCode, T? value, string error) : base(error)
    {
        ResultCode = resultCode;
        Value = value;
    }

    public static Result<T> Ok(T value) => new(ResultCode.Success, value, "");

    public static Result<T> Fail(ResultCode resultCode, string error) => new(resultCode, default, error);
    
    public override IActionResult Convert()
    {
        return ResultCode switch
        {
            ResultCode.Success => new OkObjectResult(Value),
            ResultCode.NotFound => new NotFoundObjectResult(new { error = Error }),
            ResultCode.Unauthorized => new UnauthorizedResult(), // TODO: Fix and add error message
            ResultCode.ValidationError => new BadRequestObjectResult(new { error = Error }),
            ResultCode.BadRequest => new BadRequestObjectResult(new { error = Error }),
            ResultCode.InternalServerError => new ObjectResult(new { error = Error }) { StatusCode = 500 },
            _ => new BadRequestObjectResult(new { error = Error }) 
        };
    }
}

public class Result : ResultBase
{
    private Result(ResultCode resultCode, string error) : base(error)
    {
        ResultCode = resultCode;
    }
    
    public static Result Ok() => new(ResultCode.Success, "");

    public static Result Fail(ResultCode resultCode, string error) => new(resultCode, error);
}

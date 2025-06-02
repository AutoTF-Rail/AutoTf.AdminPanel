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

    public static Result<T> Fail(ResultCode resultCode, string error = "") => new(resultCode, default, error);
    
    public static implicit operator Result<T>(T value) => Ok(value);
    
    public static implicit operator bool(Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (typeof(T) == typeof(bool) && result.Value != null)
                return bool.Parse(result.Value!.ToString()!);
        }

        return false;
    }

    public T GetValue(T replacement)
    {
        if(IsSuccess)
            return Value!;
            
        return replacement;
    }
    
    public static implicit operator bool?(Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value == null)
                return null;
            
            if (typeof(T) == typeof(bool))
                return bool.Parse(result.Value.ToString()!);
        }

        return false;
    }
    
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
    
    public static implicit operator Result(bool value)
    {
        return value ? Ok() : Fail(ResultCode.Unknown, "Operation failed.");
    }
    
    public static implicit operator bool(Result result)
    {
        return result.IsSuccess;
    }

    public static Result Fail(ResultCode resultCode, string error = "") => new(resultCode, error);
}
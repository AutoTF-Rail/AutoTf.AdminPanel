using System.Net;
using AutoTf.AdminPanel.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AutoTf.AdminPanel.Models;

/// <summary>
/// Represents the result of a method, used to report back errors easier. If it is of type object, there is no data, and the Result is only represented by the ResultCode.
/// </summary>
public class Result<T> : IConvertToActionResult
{
    public T? Value { get; }
    public string Error { get; }
    public ResultCode ResultCode { get; }

    public bool IsSuccess => ResultCode == ResultCode.Success;

    private Result(ResultCode resultCode, T? value, string error)
    {
        ResultCode = resultCode;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(ResultCode.Success, value, "");
    
    public static Result<T> Fail(ResultCode resultCode, string error) => new(resultCode, default, error);
    
    public IActionResult Convert()
    {
        return ResultCode switch
        {
            ResultCode.Success => new OkObjectResult(Value),
            ResultCode.NotFound => new NotFoundObjectResult(new { error = Error }),
            ResultCode.Unauthorized => new UnauthorizedResult(), // TODO: Fix and add error message
            ResultCode.ValidationError => new BadRequestObjectResult(new { error = Error }),
            ResultCode.InternalServerError => new ObjectResult(new { error = Error }) { StatusCode = 500 },
            _ => new BadRequestObjectResult(new { error = Error }) 
        };
    }
}

public static class Result
{
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<object> Ok() => Result<object>.Ok(new object());

    public static Result<T> Fail<T>(ResultCode resultCode, string error) => Result<T>.Fail(resultCode, error);

    public static Result<object> Fail(ResultCode resultCode, string error) => Result<object>.Fail(resultCode, error);
    
    public static ResultCode MapStatusToResultCode(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.NotFound => ResultCode.NotFound,
        HttpStatusCode.Unauthorized => ResultCode.Unauthorized,
        HttpStatusCode.BadRequest => ResultCode.ValidationError,
        _ => ResultCode.InternalServerError
    };
}
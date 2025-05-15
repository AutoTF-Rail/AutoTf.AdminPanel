using AutoTf.AdminPanel.Models.Enums;

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

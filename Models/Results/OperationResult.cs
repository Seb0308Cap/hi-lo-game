using HiLoGame.Models.Enums;

namespace HiLoGame.Models.Results;

public class OperationResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public ErrorCode? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private OperationResult(bool isSuccess, T? data, ErrorCode? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static OperationResult<T> Success(T data)
    {
        return new OperationResult<T>(true, data, null, null);
    }

    public static OperationResult<T> Failure(ErrorCode errorCode, string errorMessage)
    {
        return new OperationResult<T>(false, default, errorCode, errorMessage);
    }
}

public class OperationResult
{
    public bool IsSuccess { get; }
    public ErrorCode? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private OperationResult(bool isSuccess, ErrorCode? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static OperationResult Success()
    {
        return new OperationResult(true, null, null);
    }

    public static OperationResult Failure(ErrorCode errorCode, string errorMessage)
    {
        return new OperationResult(false, errorCode, errorMessage);
    }
}


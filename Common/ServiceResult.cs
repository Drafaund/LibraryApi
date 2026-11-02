using System.Text.Json.Serialization;
using FluentValidation.Results;

namespace LibraryApi.Common;

public class ServiceResult
{
    protected const string ValidationMessage = "Data yang dikirim tidak valid.";

    protected ServiceResult(
        bool isSuccess,
        ErrorType errorType,
        string? message,
        IReadOnlyDictionary<string, string[]>? errors)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        Message = message;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    // Only used by the controller to pick the HTTP status, not part of the response envelope.
    [JsonIgnore]
    public ErrorType ErrorType { get; }

    public string? Message { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public static ServiceResult Ok() => new(true, ErrorType.None, null, null);

    public static ServiceResult Fail(ErrorType errorType, string message) =>
        new(false, errorType, message, null);

    public static ServiceResult ValidationFail(ValidationResult validationResult) =>
        new(false, ErrorType.Validation, ValidationMessage, GroupErrors(validationResult));

    protected static Dictionary<string, string[]> GroupErrors(ValidationResult validationResult) =>
        validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
}

public class ServiceResult<T> : ServiceResult
{
    private ServiceResult(
        bool isSuccess,
        T? data,
        ErrorType errorType,
        string? message,
        IReadOnlyDictionary<string, string[]>? errors)
        : base(isSuccess, errorType, message, errors)
    {
        Data = data;
    }

    public T? Data { get; }

    public static ServiceResult<T> Ok(T data) => new(true, data, ErrorType.None, null, null);

    public static new ServiceResult<T> Fail(ErrorType errorType, string message) =>
        new(false, default, errorType, message, null);

    public static new ServiceResult<T> ValidationFail(ValidationResult validationResult) =>
        new(false, default, ErrorType.Validation, ValidationMessage, GroupErrors(validationResult));
}

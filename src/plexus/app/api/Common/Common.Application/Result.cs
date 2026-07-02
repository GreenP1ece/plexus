
namespace Common.Application;
public class Result
{
    private readonly List<string> _errors;

    internal Result(bool succeeded, List<string> errors)
    {
        Succeeded = succeeded;
        _errors = errors;
    }

    public bool Succeeded { get; }

    public List<string> Errors
        => Succeeded
            ? []
            : _errors;

    public static Result Success
        => new(true, []);

    public static Result Failure(IEnumerable<string> errors)
        => new(false, [.. errors]);

    public static implicit operator Result(string error)
        => Failure([error]);

    public static implicit operator Result(List<string> errors)
        => Failure([.. errors]);

    public static implicit operator Result(bool success)
        => success ? Success : Failure(["Unsuccessful operation."]);

    public static implicit operator bool(Result result)
        => result.Succeeded;
}

public class Result<TData> : Result
{
    private readonly TData data;

    private Result(bool succeeded, TData data, List<string> errors)
        : base(succeeded, errors)
        => this.data = data;

    public TData Data
        => Succeeded
            ? data
            : throw new InvalidOperationException(
                $"{nameof(Data)} is not available with a failed result. Use {Errors} instead.");

    public static Result<TData> SuccessWith(TData data)
        => new(true, data, []);

    public new static Result<TData> Failure(IEnumerable<string> errors)
        => new(false, default!, errors.ToList());

    public static implicit operator Result<TData>(string error)
        => Failure([error]);

    public static implicit operator Result<TData>(List<string> errors)
        => Failure(errors);

    public static implicit operator Result<TData>(TData data)
        => SuccessWith(data);

    public static implicit operator bool(Result<TData> result)
        => result.Succeeded;
}
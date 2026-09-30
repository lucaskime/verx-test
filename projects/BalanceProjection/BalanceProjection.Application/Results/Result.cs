namespace BalanceProjection.Application.Results;

/// <summary>
/// Outcome of an operation without a value: success, or one or more <see cref="Error"/>s.
/// Expected failures travel as values; exceptions stay for the truly exceptional.
/// </summary>
public class Result
{
    private static readonly Result SuccessInstance = new([]);

    protected Result(IReadOnlyList<Error> errors) => Errors = errors;

    public IReadOnlyList<Error> Errors { get; }
    public bool IsSuccess => Errors.Count == 0;
    public bool IsFailure => !IsSuccess;

    public static Result Success() => SuccessInstance;
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result Failure(Error error) => new([error]);
    public static Result Failure(IEnumerable<Error> errors) => new(RequireAny(errors));
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure([error]);
    public static Result<T> Failure<T>(IEnumerable<Error> errors) => Result<T>.Failure(RequireAny(errors));

    public static implicit operator Result(Error error) => Failure(error);

    public TOut Match<TOut>(Func<TOut> onSuccess, Func<IReadOnlyList<Error>, TOut> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Errors);

    protected static Error[] RequireAny(IEnumerable<Error> errors)
    {
        var array = errors.ToArray();
        return array.Length > 0
            ? array
            : throw new ArgumentException("A failure requires at least one error.", nameof(errors));
    }
}

/// <summary>Outcome of an operation that yields a <typeparamref name="T"/> on success.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base([]) => _value = value;
    private Result(IReadOnlyList<Error> errors) : base(errors) { }

    /// <exception cref="InvalidOperationException">The result is a failure; check <see cref="Result.IsSuccess"/> or use <see cref="Match{TOut}(Func{T, TOut}, Func{IReadOnlyList{Error}, TOut})"/>.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    internal static Result<T> Success(T value) => new(value);
    internal static Result<T> Failure(IReadOnlyList<Error> errors) => new(errors);

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new([error]);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<IReadOnlyList<Error>, TOut> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(Errors);

    public Result<TOut> Map<TOut>(Func<T, TOut> map) =>
        IsSuccess ? map(_value!) : Result<TOut>.Failure(Errors);

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind) =>
        IsSuccess ? bind(_value!) : Result<TOut>.Failure(Errors);
}

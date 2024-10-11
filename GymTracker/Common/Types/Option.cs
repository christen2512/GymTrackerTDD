namespace GymTracker.Common.Types
{
    public abstract record Option<T>;
    public sealed record Some<T>(T Value) : Option<T>;
    public sealed record None<T> : Option<T>;

    public static class OptionExtensions
    {
        public static Option<TResult> Map<T, TResult>(this Option<T> option, Func<T, TResult> map) =>
        option switch
        {
            Some<T> some => new Some<TResult>(map(some.Value)),
            _ => new None<TResult>()
        };

        public static T Reduce<T>(this Option<T> option, T whenNone) =>
        option switch
        {
            Some<T> some => some.Value,
            _ => whenNone
        };

        public static T Reduce<T>(this Option<T> option, Func<T> whenNone) =>
        option switch
        {
            Some<T> some => some.Value,
            _ => whenNone()
        };
    
    }
}
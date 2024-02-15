namespace Hackattic.Challenges.Common;

internal static class Extensions
{
    public static TOut To<TIn, TOut>(this TIn item, Func<TIn, TOut> projection) => projection(item);
}
using A55.Extensions.Configuration.Aws;

namespace A55.Extensions.Configuration;

static class InternalExtensions
{
    public static bool IsNullOrWhiteSpace(this string? str) => string.IsNullOrWhiteSpace(str);
    public static bool IsNullOrEmpty(this string? str) => string.IsNullOrEmpty(str);
    public static string JoinAsString<T>(this IEnumerable<T> items, string separador) => string.Join(separador, items);
}

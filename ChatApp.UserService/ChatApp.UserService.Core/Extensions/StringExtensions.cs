namespace ChatApp.UserService.Core.Extensions
{
    public static class StringExtensions
    {
        public static string IfNullOrEmptyUse(this string? value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}

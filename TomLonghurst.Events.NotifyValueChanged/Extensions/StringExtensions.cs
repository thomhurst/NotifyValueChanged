using System.Globalization;

namespace TomLonghurst.Events.NotifyValueChanged.Extensions;

internal static class StringExtensions
{
    public static string CapitalizeFirstLetter(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }
        
        if (char.IsUpper(s[0]))
        {
            return s;
        }

        if (s.Length == 1)
        {
            return s.ToUpper(CultureInfo.CurrentCulture);
        }

        return s.Remove(1).ToUpper(CultureInfo.CurrentCulture) + s.Substring(1);
    }
}
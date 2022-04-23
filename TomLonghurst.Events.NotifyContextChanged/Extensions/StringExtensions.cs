namespace TomLonghurst.Events.NotifyContextChanged.Extensions;

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
            return s.ToUpper();
        }

        return s.Remove(1).ToUpper() + s.Substring(1);
    }
}
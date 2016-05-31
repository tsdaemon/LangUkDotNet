using System;

namespace LangUkDotNet
{
    static class StringUtils
    {
        public static bool IsUpper(this string s)
        {
            return s.Equals(s.ToUpperInvariant());
        }

        public static bool IsUpper(this char s)
        {
            return Char.IsUpper(s);
        }
    }
}

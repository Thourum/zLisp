namespace ZLisp.Language
{
    public static class StringExtension
    {
        public static char CharAt(this string str, int index) => index > str.Length - 1 || index < 0 ? '\0' : str[index];
    }
}

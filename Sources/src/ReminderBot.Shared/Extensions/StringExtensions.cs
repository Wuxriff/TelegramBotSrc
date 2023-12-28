using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ReminderBot.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string IsoCountryCodeToFlagEmoji(this string country)
        {
            return string.Concat(country.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
        }

        public static string ToBase64(this string str)
        {
            var textBytes = Encoding.UTF8.GetBytes(str);

            return Convert.ToBase64String(textBytes);
        }

        public static string FromBase64(this string str)
        {
            var base64EncodedBytes = Convert.FromBase64String(str);

            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Truncate(this string value, int length)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return value.Length <= length ? value : value.Substring(0, length);
        }

        public static IEnumerable<string> ReadLines(this string value)
        {
            if (value == null)
                yield break;

            using var reader = new StringReader(value);
            string? line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }

        //public static string[] ToLines(this string source)
        //{
        //    return source.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        //}
    }
}

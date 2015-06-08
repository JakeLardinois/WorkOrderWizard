using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;

namespace WorkOrderWizard.Models
{
    public static class MyExtensionMethods
    {

        /*public static double? TryParse(this double? source)
        {
            double dblTemp;
            if (source.HasValue)
                return double.TryParse(source.ToString(), out dblTemp) ? dblTemp : (double?)null;
            else
                return (double?)null;
        }*/

        public static int ToInt(this double? source)
        {
            int intTemp;


            if (source.HasValue)
                return int.TryParse(source.ToString(), out intTemp) ? intTemp : 0;
            else
                return 0;
        }

        public static string EscapeSingleQuotes(this string source)
        {
            return source.Replace("\'", @"''");
        }

        public static string RemoveFirstAndLastCharacter(this string source)
        {
            return source.Substring(1, source.Length - 2);
        }

        public static string RemoveTabAndNewLine(this string source)
        {
            return source.Replace(Environment.NewLine, " ").Replace('\t', ' ');
        }

        public static string AddSingleQuotes(this string source)
        {
            string[] strArray;
            StringBuilder objStrBldr;


            strArray = source.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length > 0)
            {
                objStrBldr = new StringBuilder();

                foreach (string strTemp in strArray)
                {
                    objStrBldr.Append("'" + strTemp.Trim() + "',");
                }

                return objStrBldr.Remove(objStrBldr.Length - 1, 1).ToString();
            }
            else
                return string.Empty;


        }

        public static IOrderedEnumerable<TSource> CustomSort<TSource, TKey>(this IEnumerable<TSource> items, SortingDirection direction, Func<TSource, TKey> keySelector)
        {
            if (direction == SortingDirection.Ascending)
            {
                return items.OrderBy(keySelector);
            }

            return items.OrderByDescending(keySelector);
        }

        public static IOrderedEnumerable<TSource> CustomSort<TSource, TKey>(this IOrderedEnumerable<TSource> items, SortingDirection direction, Func<TSource, TKey> keySelector)
        {
            if (direction == SortingDirection.Ascending)
            {
                return items.ThenBy(keySelector);
            }

            return items.ThenByDescending(keySelector);
        }

        //my extension method that converts the JSON time format of /Date(1376625062603)/ to a DateTime object
        public static DateTime GetDateTimeFromJSON(this string source)
        {
            double dblTemp;

            return TimeZoneInfo.ConvertTimeFromUtc(
                new DateTime(1970, 1, 1)
                .AddMilliseconds(
                double.TryParse(
                source.Replace("/Date(", "").Replace(")/", ""),
                out dblTemp) ? dblTemp : 0),
                TimeZoneInfo.Local);
        }
    }
}
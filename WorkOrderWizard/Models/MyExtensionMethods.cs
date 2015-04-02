using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;

namespace WorkOrderWizard.Models
{
    public static class MyExtensionMethods
    {
        public static string EscapeSingleQuotes(this string source)
        {
            return source.Replace("\'", @"''");
        }

        public static string RemoveFirstAndLastCharacter(this string source)
        {
            return source.Substring(1, source.Length - 2);
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
    }
}
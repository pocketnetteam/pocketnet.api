using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace api.Extensions
{
    public static class StringExtensions
    {
        public static List<string> FromJArray(this string jArray)
        {
            var res = new List<string>();
            if (jArray == "")
            {
                return res;
            }

            try
            {
                res = JArray.Parse(jArray).ToObject<List<string>>()?
                    .Select(x => x.Replace("'", ""))
                    .ToList();
            }
            catch
            {
                //ignore
            }

            return res;
        }
        public static IEnumerable<int> AllIndicesOf(this string text, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentNullException(nameof(pattern));
            }
            return Kmp(text, pattern);
        }

        private static IEnumerable<int> Kmp(string text, string pattern)
        {
            int M = pattern.Length;
            int N = text.Length;

            int[] lps = LongestPrefixSuffix(pattern);
            int i = 0, j = 0;

            while (i < N)
            {
                if (pattern[j] == text[i])
                {
                    j++;
                    i++;
                }
                if (j == M)
                {
                    yield return i - j;
                    j = lps[j - 1];
                }

                else if (i < N && pattern[j] != text[i])
                {
                    if (j != 0)
                    {
                        j = lps[j - 1];
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        private static int[] LongestPrefixSuffix(string pattern)
        {
            int[] lps = new int[pattern.Length];
            int length = 0;
            int i = 1;

            while (i < pattern.Length)
            {
                if (pattern[i] == pattern[length])
                {
                    length++;
                    lps[i] = length;
                    i++;
                }
                else
                {
                    if (length != 0)
                    {
                        length = lps[length - 1];
                    }
                    else
                    {
                        lps[i] = length;
                        i++;
                    }
                }
            }
            return lps;
        }


        public static void getFastSearchString(this string s, string search, Dictionary<string, int> freq)
        {
            char[] delims = { ' ', ',', '(', ')', '.', '!', ':', ';', '?','\n','\t' };

            string[] searchwrds = search.ToLower().Split(delims, StringSplitOptions.RemoveEmptyEntries);
            string[] wrds = s.ToLower().Split(delims, StringSplitOptions.RemoveEmptyEntries );
            int searchwrdscnt = searchwrds.Length;

            for (int i = 0; i < wrds.Length- searchwrdscnt+1; i++)
            {
                if (wrds[i..(i+searchwrdscnt)].SequenceEqual(searchwrds))
                {
                    if (i < wrds.Length-1+ searchwrdscnt-1)
                    {
                        freq.TryGetValue(wrds[i + 1+ searchwrdscnt-1], out int value);
                        freq[wrds[i + 1+ searchwrdscnt-1]] = value + 1;
                    }
                }
            }
        }
    }
}

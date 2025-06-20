using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LibCMS.Json
{
    /// <summary>
    /// This is a custom JSON naming policy used to force property names to be in camel case, regardless of the presence of quotes.
    /// </summary>
    internal class UpperCamelNamingPolicy : JsonNamingPolicy
    {

        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            char[] array = name.ToCharArray();
            int[] dIndices, sIndices;
            array = RemoveQuotes(array, out dIndices, out sIndices);
            FixCasing(array);
            array = AddQuotes(array, dIndices, sIndices);
            return new string(array);
        }

        /// <summary>
        /// This function removes quotes from the provided Span for the purpose of fixing the casing.
        /// </summary>
        /// <param name="chars">The chars to adjust.</param>
        /// <param name="dIncides">An output array to store indices of identified double quotes.</param>
        /// <param name="sIncides">An output array to store indices of identified single quotes.</param>
        /// <returns>An array of indices where the quotes previously existed. Used for reinsertion later.</returns>
        private char[] RemoveQuotes(char[] chars, out int[] dIndices, out int[] sIndices)
        {
            dIndices = new int[0];
            sIndices = new int[0];
            List<char> charsOut = new List<char>();

            // CreateEntities the output array
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '\'')
                {
                    sIndices = sIndices.Append(i).ToArray();
                }
                else if (chars[i] == '\"')
                {
                    dIndices = dIndices.Append(i).ToArray();
                }
                else
                {
                    charsOut.Add(chars[i]);
                }
            }

            return charsOut.ToArray();
        }

        /// <summary>
        /// This function adds quotes back to the provided Span after the casing has been fixed.
        /// </summary>
        /// <param name="chars">The array of characters to add quotes.</param>
        /// <param name="dIndices">The indices where double quotes should be added</param>
        /// <param name="sIndices">The indices where single quotes should be added</param>
        /// <exception cref="NotImplementedException"></exception>
        private char[] AddQuotes(char[] chars, int[] dIndices, int[] sIndices)
        {
            List<char> charsOut = new List<char>();
            int originalLength = chars.Length + dIndices.Length + sIndices.Length;
            int insertedQuotes = 0;

            for (int i = 0; i < originalLength; i++)
            {
                if (dIndices.Contains(i))
                {
                    charsOut.Add('\"');
                    insertedQuotes++;
                }
                else if (sIndices.Contains(i))
                {
                    charsOut.Add('\'');
                    insertedQuotes++;
                }
                else
                {
                    charsOut.Add(chars[i - insertedQuotes]);
                }
            }

            return charsOut.ToArray();
        }

        /// <summary>
        /// This function adjusts the casing of the provided Span to adhere to CamelCase as best as possible.
        /// </summary>
        /// <param name="chars">The chars to adjust.</param>
        private static void FixCasing(Span<char> chars)
        {
            for (int i = 0; i < chars.Length && (i != 1 || char.IsUpper(chars[i])); i++)
            {
                bool flag = i + 1 < chars.Length;
                if (i > 0 && flag && !char.IsUpper(chars[i + 1]))
                {
                    if (chars[i + 1] == ' ')
                    {
                        chars[i] = char.ToLowerInvariant(chars[i]);
                    }

                    break;
                }

                chars[i] = char.ToLowerInvariant(chars[i]);
            }
        }
    }
}

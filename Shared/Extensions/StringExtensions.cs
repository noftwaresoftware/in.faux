using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Shared.Extensions
{
    /// <summary>
    /// String extensions/helpers.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Pluralize a word, if required.
        /// </summary>
        /// <param name="input">String to pluralize (if required).</param>
        /// <param name="count">Number of items.</param>
        /// <returns>string</returns>
        public static string Pluralize(this string input, int count)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            else
            {
                var newInput = input;
                if (input.EndsWith("y"))
                {
                    newInput = newInput.TrimEnd('y');
                    newInput += "ie";
                }

                string output;
                switch (count)
                {
                    case 0:
                        {
                            output = "No " + newInput + "s";
                            break;
                        }
                    case 1:
                        {
                            output = "1 " + input;
                            break;
                        }
                    default:
                        {
                            output = count.ToString("n0") + " " + newInput + "s";
                            break;
                        }
                } // switch (count)

                return output;
            }
        }
    }
}

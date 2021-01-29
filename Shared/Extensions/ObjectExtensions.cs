using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawn;

namespace Noftware.In.Faux.Shared.Extensions
{
    /// <summary>
    /// Extensions for object handling.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Based on the input, convert to the specified type (if possible).
        /// </summary>
        /// <param name="input">Object to be converted.</param>
        /// <returns>T or default(T), if not convertible.</returns>
        public static T ConvertTo<T>(this object input)
        {
            var returnedValue = default(T);

            if (input is not null)
            {
                if (ValueString.Of(input).Is(out T convertedValue) == true)
                {
                    returnedValue = convertedValue;
                }
            }

            return returnedValue;
        }
    }
}

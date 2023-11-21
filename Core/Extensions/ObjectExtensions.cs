// Ignore Spelling: Noftware Faux

using Dawn;

namespace Noftware.In.Faux.Core.Extensions
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

            if (input != null)
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

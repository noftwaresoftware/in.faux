using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Shared.Extensions
{
    /// <summary>
    /// Extensions for numeric type handling.
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// Based on the item count in the collection, get a random index.
        /// </summary>
        /// <typeparam name="T">Collection type.</typeparam>
        /// <param name="items">Collection.</param>
        /// <returns>Numeric index.</returns>
        public static int GetRandomIndex<T>(this IEnumerable<T> items)
        {
            return ThreadSafeRandom.Next(0, items.Count());  // Note: Inclusive, Exclusive
        }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue;
        /// that is, the range of return values includes minValue but not maxValue. If minValue
        /// equals maxValue, minValue is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">minValue is greater than maxValue.</exception>
        public static int GetRandomNumber(int minValue, int maxValue)
        {
            return ThreadSafeRandom.Next(minValue, maxValue);  // Note: Inclusive, Exclusive
        }
    }
}

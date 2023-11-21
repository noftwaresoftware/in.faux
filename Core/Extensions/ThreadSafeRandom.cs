// Ignore Spelling: Noftware Faux

using System;
using System.Threading;

namespace Noftware.In.Faux.Core.Extensions
{
    /// <summary>
    /// A thread-safe random number implementation.
    /// </summary>
    public static class ThreadSafeRandom
    {
        // https://stackoverflow.com/questions/3049467/is-c-sharp-random-number-generator-thread-safe/24648788#24648788

        // Random number generator
        private static readonly System.Random _globalRandom = new();

        // ThreadLocal: Provides thread-local storage of data.
        private static readonly ThreadLocal<Random> _localRandom = new(() =>
        {
            lock (_globalRandom)
            {
                return new Random(_globalRandom.Next());
            }
        });

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
        public static int Next(int minValue = 0, int maxValue = Int32.MaxValue)
        {
            return _localRandom.Value.Next(minValue, maxValue);
        }
    }
}

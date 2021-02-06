using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In.Faux.BulkUploader
{
    /// <summary>
    /// Helper methods to support the console application.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Get a list of command-line arguments to display to the user.
        /// </summary>
        /// <returns><see cref="IEnumerable{string}"/></returns>
        public static IEnumerable<string> GetArgumentInformation()
        {
            var argItems = new List<string>();
            argItems.Add("Valid modes:");
            argItems.Add("/a = Append to existing Azure table storage and file shares.");
            argItems.Add("/o = Overwrite existing Azure table storage and file shares with new data.");

            return argItems;
        }

        /// <summary>
        /// Get a list of keyboard inputs to display to the user.
        /// </summary>
        /// <returns><see cref="IEnumerable{string}"/></returns>
        public static IEnumerable<string> GetInputInformation()
        {
            var inputItems = new List<string>();
            inputItems.Add("append = Append to existing Azure table storage and file shares.");
            inputItems.Add("overwrite = Overwrite existing Azure table storage and file shares with new data.");

            return inputItems;
        }
    }
}

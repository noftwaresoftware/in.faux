using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.BulkUploader
{
    /// <summary>
    /// Helper methods to support the console application.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Get a list of command-line arguments to display to the user.
        /// </summary>
        /// <returns><see cref="IEnumerable"/></returns>
        public static IEnumerable<string> GetArgumentInformation()
        {
            var argItems = new List<string>()
            {
                "Valid modes:",
                "/a = Append to existing Azure table storage and file shares.",
                "/o = Overwrite existing Azure table storage and file shares with new data."
            };

            return argItems;
        }

        /// <summary>
        /// Get a list of keyboard inputs to display to the user.
        /// </summary>
        /// <returns><see cref="IEnumerable"/></returns>
        public static IEnumerable<string> GetInputInformation()
        {
            var inputItems = new List<string>()
            {
                "append = Append to existing Azure table storage and file shares.",
                "overwrite = Overwrite existing Azure table storage and file shares with new data."
            };

            return inputItems;
        }
    }
}

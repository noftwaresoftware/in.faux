using Azure.Storage.Files.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Core.Models
{
    /// <summary>
    /// Represents the base class of a file.
    /// </summary>
    public abstract class File
    {
        /// <summary>
        /// File name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Contents of file.
        /// </summary>
        public byte[] Contents { get; set; }
    }
}

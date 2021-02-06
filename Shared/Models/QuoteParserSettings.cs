using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Shared.Models
{
	/// <summary>
	/// Settings for the quote flat file parser and image resizing.
	/// </summary>
	public class QuoteParserSettings
	{
		/// <summary>
		/// Maximum resized dimension in pixels.
		/// </summary>
		public double MaximumResizedImageDimension { get; set; }

		/// <summary>
		/// Maximum thumbnail dimension in pixels.
		/// </summary>
		public double MaximumThumbnailImageDimension { get; set; }

		/// <summary>
		/// Path to where all images are located.
		/// </summary>
		public string InputImagePath { get; set; }

		/// <summary>
		/// Path and name of quote source file.
		/// </summary>
		public string QuoteTextFile { get; set; }
	}
}
